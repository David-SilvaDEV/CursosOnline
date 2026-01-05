namespace CursosOnline.Application.DTOs.CursosOnline;

/// <summary>
/// DTO para crear cursos (solo datos necesarios desde el cliente).
/// </summary>
public class CourseCreateDto
{
    public string Title { get; set; } = string.Empty;
}

/// <summary>
/// DTO para actualizar cursos (solo campos que el cliente puede modificar).
/// </summary>
public class CourseUpdateDto
{
    public string Title { get; set; } = string.Empty;
    /// <summary>
    /// "Draft" | "Published". No exponer flags internos ni campos sensibles.
    /// </summary>
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// DTO para listar cursos (vista resumida).
/// </summary>
public class CourseListDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// DTO de detalle de curso devuelto al cliente.
/// No expone banderas internas como IsDeleted ni datos internos de infraestructura.
/// </summary>
public class CourseResponseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
