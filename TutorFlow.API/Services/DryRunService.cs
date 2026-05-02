using System.Text.RegularExpressions;

namespace TutorFlow.API.Services;

public class DryRunService
{
    // Matches: x = 5 | name = "Ali" | result = x + y
    private static readonly Regex AssignmentRegex =
        new(@"^([a-zA-Z_][a-zA-Z0-9_]*)\s*=\s*(.+)$", RegexOptions.Compiled);

    // Matches: print(...)  or  print("hello")
    private static readonly Regex PrintRegex =
        new(@"^print\s*\((.+)\)\s*$", RegexOptions.Compiled);

    // Matches augmented assignment: x += 1 | x -= 2 | x *= 3 | x //= 2
    private static readonly Regex AugmentedAssignRegex =
        new(@"^([a-zA-Z_][a-zA-Z0-9_]*)\s*(\+|-|\*|\/\/|\/|\*\*|%|&|\||\^)=\s*(.+)$", RegexOptions.Compiled);

    public List<DryRunStep> Simulate(string code)
    {
        var steps = new List<DryRunStep>();
        var variables = new Dictionary<string, object?>();
        var lines = code.Split('\n');

        for (int i = 0; i < lines.Length; i++)
        {
            var raw = lines[i];
            var trimmed = raw.Trim();
            int lineNumber = i + 1;

            // Skip empty lines and comments
            if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith('#'))
            {
                steps.Add(new DryRunStep
                {
                    Line = lineNumber,
                    Code = trimmed,
                    StepType = trimmed.StartsWith('#') ? "comment" : "empty"
                });
                continue;
            }

            // ── Augmented assignment: x += 1 ──────────────────────────────
            var augMatch = AugmentedAssignRegex.Match(trimmed);
            if (augMatch.Success)
            {
                var varName = augMatch.Groups[1].Value;
                var op = augMatch.Groups[2].Value;
                var rhs = augMatch.Groups[3].Value.Trim();

                var rhsVal = Evaluate(rhs, variables);
                var currentVal = variables.ContainsKey(varName) ? variables[varName] : 0.0;
                var newVal = ApplyAugmented(currentVal, op, rhsVal);

                variables[varName] = newVal;
                steps.Add(new DryRunStep
                {
                    Line = lineNumber,
                    Code = trimmed,
                    Variable = varName,
                    Value = FormatValue(newVal),
                    StepType = "assignment"
                });
                continue;
            }

            // ── Regular assignment: x = 5 ─────────────────────────────────
            var assignMatch = AssignmentRegex.Match(trimmed);
            if (assignMatch.Success)
            {
                var varName = assignMatch.Groups[1].Value;
                var expression = assignMatch.Groups[2].Value.Trim();

                // Skip function definitions that look like assignments
                if (expression.StartsWith("lambda") || trimmed.StartsWith("def "))
                {
                    steps.Add(new DryRunStep { Line = lineNumber, Code = trimmed, StepType = "unknown" });
                    continue;
                }

                var value = Evaluate(expression, variables);
                variables[varName] = value;

                steps.Add(new DryRunStep
                {
                    Line = lineNumber,
                    Code = trimmed,
                    Variable = varName,
                    Value = FormatValue(value),
                    StepType = "assignment"
                });
                continue;
            }

            // ── Print statement: print(...) ───────────────────────────────
            var printMatch = PrintRegex.Match(trimmed);
            if (printMatch.Success)
            {
                var inner = printMatch.Groups[1].Value.Trim();
                var printVal = EvaluatePrint(inner, variables);

                steps.Add(new DryRunStep
                {
                    Line = lineNumber,
                    Code = trimmed,
                    Output = printVal,
                    StepType = "print"
                });
                continue;
            }

            // ── Unrecognised line ─────────────────────────────────────────
            steps.Add(new DryRunStep
            {
                Line = lineNumber,
                Code = trimmed,
                StepType = "unknown"
            });
        }

        return steps;
    }

    // ── Expression evaluator ──────────────────────────────────────────────

    private object? Evaluate(string expression, Dictionary<string, object?> vars)
    {
        expression = expression.Trim();

        // String literal: "hello" or 'hello'
        if ((expression.StartsWith('"') && expression.EndsWith('"')) ||
            (expression.StartsWith('\'') && expression.EndsWith('\'')))
            return expression[1..^1];

        // Boolean literals
        if (expression == "True") return true;
        if (expression == "False") return false;
        if (expression == "None") return null;

        // Numeric literal
        if (double.TryParse(expression, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out double num))
            return num;

        // Variable reference
        if (vars.ContainsKey(expression))
            return vars[expression];

        // String concatenation / arithmetic: try to resolve a two-operand expression
        return TryEvaluateBinaryExpression(expression, vars);
    }

    private object? TryEvaluateBinaryExpression(string expression, Dictionary<string, object?> vars)
    {
        // Supported operators in precedence order (right to left for splitting)
        var operators = new[] { "+", "-", "*", "//", "/", "%", "**" };

        foreach (var op in operators)
        {
            var idx = FindOperatorIndex(expression, op);
            if (idx < 0) continue;

            var leftStr = expression[..idx].Trim();
            var rightStr = expression[(idx + op.Length)..].Trim();

            var left = Evaluate(leftStr, vars);
            var right = Evaluate(rightStr, vars);

            // String concatenation
            if (op == "+" && (left is string || right is string))
                return $"{left}{right}";

            // Numeric operations
            if (TryToDouble(left, out double l) && TryToDouble(right, out double r))
            {
                return op switch
                {
                    "+" => l + r,
                    "-" => l - r,
                    "*" => l * r,
                    "/" => r != 0 ? l / r : (object?)"DivisionByZero",
                    "//" => r != 0 ? Math.Floor(l / r) : (object?)"DivisionByZero",
                    "%" => r != 0 ? l % r : (object?)"DivisionByZero",
                    "**" => Math.Pow(l, r),
                    _ => null
                };
            }
        }

        // Fall back: return the raw expression string
        return expression;
    }

    private string EvaluatePrint(string inner, Dictionary<string, object?> vars)
    {
        // Handle f-strings: f"Hello {name}"
        if (inner.StartsWith("f\"") || inner.StartsWith("f'"))
        {
            var template = inner[2..^1];
            return Regex.Replace(template, @"\{([^}]+)\}", match =>
            {
                var varName = match.Groups[1].Value.Trim();
                return vars.ContainsKey(varName) ? FormatValue(vars[varName]) : match.Value;
            });
        }

        // Handle multiple arguments: print(x, y)
        if (inner.Contains(','))
        {
            var parts = SplitByComma(inner);
            return string.Join(" ", parts.Select(p => FormatValue(Evaluate(p.Trim(), vars))));
        }

        return FormatValue(Evaluate(inner, vars));
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private object? ApplyAugmented(object? current, string op, object? rhs)
    {
        if (TryToDouble(current, out double l) && TryToDouble(rhs, out double r))
        {
            return op switch
            {
                "+" => l + r,
                "-" => l - r,
                "*" => l * r,
                "/" => r != 0 ? l / r : (object?)"DivisionByZero",
                "//" => r != 0 ? Math.Floor(l / r) : (object?)"DivisionByZero",
                "%" => l % r,
                "**" => Math.Pow(l, r),
                _ => current
            };
        }
        if (op == "+" && (current is string || rhs is string))
            return $"{current}{rhs}";
        return current;
    }

    private static int FindOperatorIndex(string expr, string op)
    {
        // Find operator outside of parentheses/quotes — search right-to-left for left-associativity
        int depth = 0;
        bool inStr = false;
        char strChar = '"';

        for (int i = expr.Length - op.Length; i >= 0; i--)
        {
            char c = expr[i];
            if (!inStr && (c == ')' || c == ']')) depth++;
            if (!inStr && (c == '(' || c == '[')) depth--;
            if (!inStr && (c == '"' || c == '\'')) { inStr = true; strChar = c; }
            else if (inStr && c == strChar) inStr = false;
            if (depth == 0 && !inStr && expr[i..].StartsWith(op))
            {
                // Don't match ** when looking for *
                if (op == "*" && i + 1 < expr.Length && expr[i + 1] == '*') continue;
                if (op == "/" && i + 1 < expr.Length && expr[i + 1] == '/') continue;
                if (i > 0 && (expr[i - 1] == '*' || expr[i - 1] == '/')) continue;
                return i;
            }
        }
        return -1;
    }

    private static bool TryToDouble(object? val, out double result)
    {
        if (val is double d) { result = d; return true; }
        if (val is int i) { result = i; return true; }
        if (val is bool b) { result = b ? 1 : 0; return true; }
        result = 0;
        return false;
    }

    private static string FormatValue(object? val) => val switch
    {
        null => "None",
        bool b => b ? "True" : "False",
        double d => d == Math.Floor(d) ? ((long)d).ToString() : d.ToString("G6"),
        _ => val.ToString() ?? "None"
    };

    private static List<string> SplitByComma(string s)
    {
        var parts = new List<string>();
        int depth = 0; bool inStr = false; char strChar = '"'; int start = 0;
        for (int i = 0; i < s.Length; i++)
        {
            char c = s[i];
            if (!inStr && (c == '(' || c == '[')) depth++;
            if (!inStr && (c == ')' || c == ']')) depth--;
            if (!inStr && (c == '"' || c == '\'')) { inStr = true; strChar = c; }
            else if (inStr && c == strChar) inStr = false;
            if (c == ',' && depth == 0 && !inStr)
            {
                parts.Add(s[start..i]);
                start = i + 1;
            }
        }
        parts.Add(s[start..]);
        return parts;
    }
}
