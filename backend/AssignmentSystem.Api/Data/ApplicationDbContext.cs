using AssignmentSystem.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssignmentSystem.Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Class> Classes => Set<Class>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<TeacherClassSubject> TeacherClassSubjects => Set<TeacherClassSubject>();
    public DbSet<Assignment> Assignments => Set<Assignment>();
    public DbSet<Submission> Submissions => Set<Submission>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ---------- User ----------
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();

            entity.Property(u => u.Role)
                .HasConversion<string>() // store enum as readable text, not a magic int, in the DB
                .HasMaxLength(20);

            // A student's ClassId -> Class.Id. RESTRICT: an admin must reassign/remove
            // students before a class can be deleted, rather than EF silently cascading.
            entity.HasOne(u => u.Class)
                .WithMany(c => c.Students)
                .HasForeignKey(u => u.ClassId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- Class ----------
        modelBuilder.Entity<Class>(entity =>
        {
            entity.Property(c => c.Name).HasMaxLength(150).IsRequired();
        });

        // ---------- Subject ----------
        modelBuilder.Entity<Subject>(entity =>
        {
            entity.Property(s => s.Name).HasMaxLength(150).IsRequired();
            entity.HasIndex(s => s.Code).IsUnique().HasFilter("\"Code\" IS NOT NULL");
        });

        // ---------- TeacherClassSubject ----------
        modelBuilder.Entity<TeacherClassSubject>(entity =>
        {
            // The single most important constraint in the schema for authorization:
            // a teacher cannot hold the exact same (class, subject) grant twice.
            entity.HasIndex(t => new { t.TeacherId, t.ClassId, t.SubjectId }).IsUnique();

            entity.HasOne(t => t.Teacher)
                .WithMany(u => u.TeachingGrants)
                .HasForeignKey(t => t.TeacherId)
                .OnDelete(DeleteBehavior.Cascade); // deleting a teacher account removes their grants

            entity.HasOne(t => t.Class)
                .WithMany(c => c.TeacherGrants)
                .HasForeignKey(t => t.ClassId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(t => t.Subject)
                .WithMany(s => s.TeacherGrants)
                .HasForeignKey(t => t.SubjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ---------- Assignment ----------
        modelBuilder.Entity<Assignment>(entity =>
        {
            entity.Property(a => a.Title).HasMaxLength(200).IsRequired();
            entity.Property(a => a.Description).IsRequired();
            entity.Property(a => a.MaxMarks).HasColumnType("decimal(6,2)");

            entity.Property(a => a.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            // Highest-traffic read path: "student's available assignments" filters
            // on exactly these three columns together.
            entity.HasIndex(a => new { a.ClassId, a.Status, a.IsDeleted });

            // None of these cascade-delete: deleting a Class/Subject/User should
            // never silently wipe assignment history. Blocked at the DB level
            // (admin must handle reassignment/soft-delete first).
            entity.HasOne(a => a.Class)
                .WithMany(c => c.Assignments)
                .HasForeignKey(a => a.ClassId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.Subject)
                .WithMany(s => s.Assignments)
                .HasForeignKey(a => a.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.CreatedByTeacher)
                .WithMany(u => u.AssignmentsCreated)
                .HasForeignKey(a => a.CreatedByTeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            // Global query filter: soft-deleted assignments are invisible to every
            // normal query unless explicitly bypassed with IgnoreQueryFilters().
            // This is a deliberate defense-in-depth measure - even if a service
            // method forgets to check IsDeleted, the query filter still excludes it.
            entity.HasQueryFilter(a => !a.IsDeleted);
        });

        // ---------- Submission ----------
        modelBuilder.Entity<Submission>(entity =>
        {
            entity.Property(s => s.AnswerText).IsRequired();
            entity.Property(s => s.Marks).HasColumnType("decimal(5,2)");

            entity.Property(s => s.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            // Enforces "one submission per student per assignment" at the DB level,
            // not just in application code (defense-in-depth, see Phase 1 §4).
            entity.HasIndex(s => new { s.AssignmentId, s.StudentId }).IsUnique();

            entity.HasIndex(s => s.AssignmentId); // teacher's "submissions for my assignment" query
            entity.HasIndex(s => s.StudentId);    // student's "my submissions" query

            entity.HasOne(s => s.Assignment)
                .WithMany(a => a.Submissions)
                .HasForeignKey(s => s.AssignmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(s => s.Student)
                .WithMany(u => u.Submissions)
                .HasForeignKey(s => s.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(s => s.ReviewedByTeacher)
                .WithMany()
                .HasForeignKey(s => s.ReviewedByTeacherId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
