using Microsoft.AspNetCore.Identity;
using TutorFlow.Core.Entities;
using TutorFlow.Core.Enums;

namespace TutorFlow.Infrastructure;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<Student> Students { get; set; } = new List<Student>();
}