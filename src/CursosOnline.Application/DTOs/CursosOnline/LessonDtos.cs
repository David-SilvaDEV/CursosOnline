namespace CursosOnline.Application.DTOs.CursosOnline;

public class LessonCreateDto
{
    public string Title { get; set; } = string.Empty;
}

public class LessonUpdateDto
{
    public string Title { get; set; } = string.Empty;
}

public class LessonResponseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Order { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
