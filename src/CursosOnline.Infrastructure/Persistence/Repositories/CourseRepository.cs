using CursosOnline.Domain.Entities;
using CursosOnline.Domain.Interfaces;
using CursosOnline.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace CursosOnline.Infrastructure.Persistence.Repositories;

public class CourseRepository : ICourseRepository
{
    private readonly ApplicationDbContext _context;

    public CourseRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Course?> GetByIdAsync(Guid id)
    {
        return await _context.Courses
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
    }

    Task<IEnumerable<Course>> ICourseRepository.GetAllAsync()
    {
        return GetAllAsync();
    }

    Task<IEnumerable<Course>> ICourseRepository.GetPublishedAsync()
    {
        return GetPublishedAsync();
    }

    Task ICourseRepository.AddAsync(Course course)
    {
        return AddAsync(course);
    }

    Task ICourseRepository.UpdateAsync(Course course)
    {
        return UpdateAsync(course);
    }

    Task<Course?> ICourseRepository.GetByIdAsync(Guid id)
    {
        return GetByIdAsync(id);
    }

    public async Task<IEnumerable<Course>> GetAllAsync()
    {
        return await _context.Courses
            .Where(c => !c.IsDeleted)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Course>> GetPublishedAsync()
    {
        return await _context.Courses
            .Where(c => !c.IsDeleted && c.Status == CourseStatus.Published)
            .ToListAsync();
    }

    public async Task<(IEnumerable<Course> Items, int TotalCount)> SearchAsync(string? query, CourseStatus? status, int page, int pageSize)
    {
        var q = _context.Courses
            .Where(c => !c.IsDeleted);

        if (!string.IsNullOrWhiteSpace(query))
        {
            var lowered = query.Trim().ToLower();
            q = q.Where(c => c.Title.ToLower().Contains(lowered));
        }

        if (status.HasValue)
        {
            q = q.Where(c => c.Status == status.Value);
        }

        var total = await q.CountAsync();

        var items = await q
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task AddAsync(Course course)
    {
        await _context.Courses.AddAsync(course);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Course course)
    {
        course.UpdatedAt = DateTime.UtcNow;
        _context.Courses.Update(course);
        await _context.SaveChangesAsync();
    }

    public async Task SoftDeleteAsync(Guid id)
    {
        var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == id);

        if (course == null) return;

        course.IsDeleted = true;
        course.UpdatedAt = DateTime.UtcNow;

        _context.Courses.Update(course);
        await _context.SaveChangesAsync();
    }
}