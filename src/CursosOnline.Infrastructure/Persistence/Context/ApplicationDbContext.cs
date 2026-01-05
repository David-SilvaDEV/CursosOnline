using CursosOnline.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CursosOnline.Infrastructure.Persistence.Context;

public class ApplicationDbContext 
    : IdentityDbContext<IdentityUser, IdentityRole, string>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // 🔹 DbSets de tu dominio
    public DbSet<Course> Courses { get; set; }
    public DbSet<Lesson> Lessons { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // 🔹 Configuración de Course
        builder.Entity<Course>(entity =>
        {
            entity.HasKey(c => c.Id);

            entity.Property(c => c.Title)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(c => c.Status)
                .HasConversion<string>(); // Guarda enum como string

            entity.Property(c => c.CreatedAt)
                .IsRequired();

            entity.Property(c => c.UpdatedAt)
                .IsRequired();

            // 🔹 Soft Delete (filtro global)
            entity.HasQueryFilter(c => !c.IsDeleted);

            // Relación uno a muchos con Lesson
            entity.HasMany(c => c.Lessons)
                .WithOne(l => l.Course!)
                .HasForeignKey(l => l.CourseId);
        });

        // 🔹 Configuración de Lesson
        builder.Entity<Lesson>(entity =>
        {
            entity.HasKey(l => l.Id);

            entity.Property(l => l.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(l => l.Order)
                .IsRequired();

            entity.Property(l => l.CreatedAt)
                .IsRequired();

            entity.Property(l => l.UpdatedAt)
                .IsRequired();

            // Filtro global para soft delete
            entity.HasQueryFilter(l => !l.IsDeleted);

            // Order único dentro de un curso
            entity.HasIndex(l => new { l.CourseId, l.Order })
                .IsUnique();
        });
    }
}
