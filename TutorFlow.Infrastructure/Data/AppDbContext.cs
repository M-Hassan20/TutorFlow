using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TutorFlow.Core.Entities;

namespace TutorFlow.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Student> Students => Set<Student>();
    public DbSet<Assignment> Assignments => Set<Assignment>();
    public DbSet<Submission> Submissions => Set<Submission>();
    public DbSet<Badge> Badges => Set<Badge>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Student → Tutor
        builder.Entity<Student>()
            .HasOne<ApplicationUser>()
            .WithMany(u => u.Students)
            .HasForeignKey(s => s.TutorId)
            .OnDelete(DeleteBehavior.Cascade);

        // Assignment → Tutor
        builder.Entity<Assignment>()
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(a => a.TutorId)
            .OnDelete(DeleteBehavior.Cascade);

        // Submission → Student
        builder.Entity<Submission>()
            .HasOne(s => s.Student)
            .WithMany(st => st.Submissions)
            .HasForeignKey(s => s.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Submission → Assignment
        builder.Entity<Submission>()
            .HasOne(s => s.Assignment)
            .WithMany(a => a.Submissions)
            .HasForeignKey(s => s.AssignmentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Assignment ↔ Student (many-to-many)
        builder.Entity<Assignment>()
            .HasMany(a => a.Students)
            .WithMany(s => s.Assignments)
            .UsingEntity(j => j.ToTable("AssignmentStudents"));

        // Badge → Student
        builder.Entity<Badge>()
            .HasOne(b => b.Student)
            .WithMany(s => s.Badges)
            .HasForeignKey(b => b.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        // ApplicationUserId is a simple nullable string FK (no nav property needed)
        builder.Entity<Student>()
            .Property(s => s.ApplicationUserId)
            .IsRequired(false);
    }
}
