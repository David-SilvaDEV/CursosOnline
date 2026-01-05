namespace CursosOnline.Application.DTOs.CursosOnline;

public class CourseSummaryDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int TotalLessons { get; set; }
    public DateTime LastModifiedAt { get; set; }
}
