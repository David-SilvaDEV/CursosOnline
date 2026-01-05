using CursosOnline.Domain.Entities;

namespace CursosOnline.Domain.Interfaces;

public interface ICourseRepository
{
    Task<Course?> GetByIdAsync(Guid id);
    Task<IEnumerable<Course>> GetAllAsync();
    Task<IEnumerable<Course>> GetPublishedAsync();

    Task AddAsync(Course course);
    Task UpdateAsync(Course course);
    Task SoftDeleteAsync(Guid id);

    /// <summary>
    /// Búsqueda de cursos con filtro opcional por texto y estado, con paginación.
    /// </summary>
    Task<(IEnumerable<Course> Items, int TotalCount)> SearchAsync(string? query, CourseStatus? status, int page, int pageSize);
}
