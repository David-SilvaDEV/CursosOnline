using System.ComponentModel.DataAnnotations;

namespace CursosOnline.Domain.Entities;

public class Course

{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public CourseStatus Status { get; set; }

    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Relación: un curso tiene muchas lecciones
    public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
}

public enum CourseStatus
{
    Draft = 0,
    Published = 1
}

public class Lesson
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid CourseId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public int Order { get; set; }

    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navegación
    public Course? Course { get; set; }
}
