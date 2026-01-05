using CursosOnline.Domain.Entities;

namespace CursosOnline.Domain.Interfaces;

public interface ILessonRepository
{
    Task<Lesson?> GetByIdAsync(Guid id);
    Task<IEnumerable<Lesson>> GetByCourseAsync(Guid courseId);

    Task AddAsync(Lesson lesson);
    Task UpdateAsync(Lesson lesson);
    Task SoftDeleteAsync(Guid id);

    /// <summary>
    /// Sube la lección una posición dentro de su curso (disminuye Order).
    /// </summary>
    Task MoveUpAsync(Guid lessonId);

    /// <summary>
    /// Baja la lección una posición dentro de su curso (aumenta Order).
    /// </summary>
    Task MoveDownAsync(Guid lessonId);
}