using CursosOnline.Domain.Entities;
using CursosOnline.Domain.Interfaces;
using CursosOnline.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace CursosOnline.Infrastructure.Persistence.Repositories;

public class LessonRepository : ILessonRepository
{
    private readonly ApplicationDbContext _context;

    public LessonRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Lesson?> GetByIdAsync(Guid id)
    {
        return await _context.Lessons
            .FirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted);
    }

    public async Task<IEnumerable<Lesson>> GetByCourseAsync(Guid courseId)
    {
        return await _context.Lessons
            .Where(l => l.CourseId == courseId && !l.IsDeleted)
            .OrderBy(l => l.Order)
            .ToListAsync();
    }

    public async Task AddAsync(Lesson lesson)
    {
        await _context.Lessons.AddAsync(lesson);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Lesson lesson)
    {
        lesson.UpdatedAt = DateTime.UtcNow;
        _context.Lessons.Update(lesson);
        await _context.SaveChangesAsync();
    }

    public async Task SoftDeleteAsync(Guid id)
    {
        var lesson = await _context.Lessons.FirstOrDefaultAsync(l => l.Id == id);
        if (lesson == null) return;

        lesson.IsDeleted = true;
        lesson.UpdatedAt = DateTime.UtcNow;

        _context.Lessons.Update(lesson);
        await _context.SaveChangesAsync();
    }

    public async Task MoveUpAsync(Guid lessonId)
    {
        var lesson = await GetByIdAsync(lessonId);
        if (lesson == null) return;

        var previous = await _context.Lessons
            .Where(l => l.CourseId == lesson.CourseId && !l.IsDeleted && l.Order < lesson.Order)
            .OrderByDescending(l => l.Order)
            .FirstOrDefaultAsync();

        if (previous == null) return; // ya está arriba del todo

        var tempOrder = lesson.Order;
        lesson.Order = previous.Order;
        previous.Order = tempOrder;

        lesson.UpdatedAt = DateTime.UtcNow;
        previous.UpdatedAt = DateTime.UtcNow;

        _context.Lessons.UpdateRange(lesson, previous);
        await _context.SaveChangesAsync();
    }

    public async Task MoveDownAsync(Guid lessonId)
    {
        var lesson = await GetByIdAsync(lessonId);
        if (lesson == null) return;

        var next = await _context.Lessons
            .Where(l => l.CourseId == lesson.CourseId && !l.IsDeleted && l.Order > lesson.Order)
            .OrderBy(l => l.Order)
            .FirstOrDefaultAsync();

        if (next == null) return; // ya está abajo del todo

        var tempOrder = lesson.Order;
        lesson.Order = next.Order;
        next.Order = tempOrder;

        lesson.UpdatedAt = DateTime.UtcNow;
        next.UpdatedAt = DateTime.UtcNow;

        _context.Lessons.UpdateRange(lesson, next);
        await _context.SaveChangesAsync();
    }
}
