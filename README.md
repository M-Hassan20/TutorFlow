# TutorFlow API — Phase 1

Vertical SaaS backend for independent coding instructors. Built with ASP.NET Core 8, EF Core, SQLite, and JWT auth.

## Project Structure

```
TutorFlow/
├── TutorFlow.sln
├── TutorFlow.Core/              # Domain layer — Entities, Interfaces, Enums
│   ├── Entities/
│   │   ├── ApplicationUser.cs
│   │   ├── Student.cs
│   │   ├── Assignment.cs
│   │   └── Submission.cs
│   ├── Interfaces/
│   │   ├── IStudentRepository.cs
│   │   ├── IAssignmentRepository.cs
│   │   └── ISubmissionRepository.cs
│   └── Enums/
│       └── UserRole.cs
├── TutorFlow.Infrastructure/    # Data layer — EF Core, Repositories
│   ├── Data/
│   │   └── AppDbContext.cs
│   └── Repositories/
│       ├── StudentRepository.cs
│       ├── AssignmentRepository.cs
│       └── SubmissionRepository.cs
└── TutorFlow.API/               # Presentation layer — Controllers, DTOs
    ├── Controllers/
    │   ├── AuthController.cs
    │   ├── StudentsController.cs
    │   ├── AssignmentsController.cs
    │   └── SubmissionsController.cs
    ├── DTOs/
    │   ├── AuthDtos.cs
    │   ├── StudentDtos.cs
    │   ├── AssignmentDtos.cs
    │   └── SubmissionDtos.cs
    ├── Program.cs
    └── appsettings.json
```

## Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- [EF Core tools](https://learn.microsoft.com/en-us/ef/core/cli/dotnet)

```bash
dotnet tool install --global dotnet-ef
```

### 1. Restore packages

```bash
dotnet restore TutorFlow.sln
```

### 2. Apply database migrations

```bash
# Create the initial migration
dotnet ef migrations add InitialCreate \
  --project TutorFlow.Infrastructure \
  --startup-project TutorFlow.API

# Apply it (creates tutorflow.db)
dotnet ef database update \
  --project TutorFlow.Infrastructure \
  --startup-project TutorFlow.API
```

### 3. Run the API

```bash
cd TutorFlow.API
dotnet run
```

Swagger UI will open at: **http://localhost:5000**

---

## API Endpoints

### Auth
| Method | Route | Description | Auth |
|--------|-------|-------------|------|
| POST | `/api/auth/register` | Register a new user | ❌ |
| POST | `/api/auth/login` | Login, receive JWT | ❌ |

### Students *(Tutor only)*
| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/students` | Get all your students |
| GET | `/api/students/{id}` | Get student by ID |
| POST | `/api/students` | Enroll a new student |
| PUT | `/api/students/{id}` | Update student info |
| DELETE | `/api/students/{id}` | Soft-delete student |

### Assignments *(Tutor only)*
| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/assignments` | Get all your assignments |
| GET | `/api/assignments/{id}` | Get assignment by ID |
| POST | `/api/assignments` | Create assignment |
| PUT | `/api/assignments/{id}` | Update assignment |
| DELETE | `/api/assignments/{id}` | Delete assignment |

### Submissions
| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/submissions/student/{id}` | Get a student's submissions |
| GET | `/api/submissions/assignment/{id}` | Get assignment submissions |
| POST | `/api/submissions` | Submit code (Phase 2: will execute via Piston API) |

---

## Quick Test Flow (using Swagger)

1. **Register** a tutor → `POST /api/auth/register`
   ```json
   { "firstName": "Ali", "lastName": "Khan", "email": "ali@example.com", "password": "Test1234", "role": "Tutor" }
   ```

2. **Copy the JWT token** from the response

3. **Click "Authorize"** in Swagger UI, paste `Bearer <token>`

4. **Create a student** → `POST /api/students`
   ```json
   { "firstName": "Sara", "lastName": "Ahmed", "age": 12, "parentEmail": "parent@example.com" }
   ```

5. **Create an assignment** → `POST /api/assignments`
   ```json
   { "title": "Hello World", "description": "Print your name", "starterCode": "# Write your code here", "language": "python", "xpReward": 50, "dueDate": null }
   ```

---

## What's Next — Phase 2

- [ ] Connect `POST /api/submissions` to the **Piston API** for real code execution
- [ ] Integrate **Monaco Editor** on the frontend
- [ ] Return structured `Output` and auto-grade `IsCorrect`

## Phase 3 (Dry-Run Engine)
- [ ] Build a C# Python statement simulator
- [ ] Return step-by-step variable tracking as JSON

## Phase 4 (Gamification)
- [ ] XP system is already wired into `SubmissionRepository`
- [ ] Add `GET /api/students/{id}/progress` endpoint
- [ ] Build parent-facing dashboard
