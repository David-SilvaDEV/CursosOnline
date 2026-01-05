using CursosOnline.Application.DTOs.Common;
using CursosOnline.Application.DTOs.CursosOnline;
using CursosOnline.Domain.Entities;
using CursosOnline.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CursosOnline.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/courses/{courseId:guid}/[controller]")]
public class LessonsController : ControllerBase
{
    private readonly ILessonRepository _lessonRepository;
    private readonly ICourseRepository _courseRepository;

    public LessonsController(ILessonRepository lessonRepository, ICourseRepository courseRepository)
    {
        _lessonRepository = lessonRepository;
        _courseRepository = courseRepository;
    }

    // GET: api/courses/{courseId}/lessons
    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<LessonResponseDto>>>> GetByCourse(Guid courseId)
    {
        var course = await _courseRepository.GetByIdAsync(courseId);
        if (course is null)
        {
            return NotFound(new ApiResponseDto<IEnumerable<LessonResponseDto>>
            {
                Success = false,
                Message = "Curso no encontrado"
            });
        }

        var lessons = await _lessonRepository.GetByCourseAsync(courseId);

        var data = lessons.Select(l => new LessonResponseDto
        {
            Id = l.Id,
            Title = l.Title,
            Order = l.Order,
            CreatedAt = l.CreatedAt,
            UpdatedAt = l.UpdatedAt
        });

        return Ok(new ApiResponseDto<IEnumerable<LessonResponseDto>>
        {
            Success = true,
            Message = "Listado de lecciones",
            Data = data
        });
    }

    // POST: api/courses/{courseId}/lessons
    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<LessonResponseDto>>> Create(Guid courseId, [FromBody] LessonCreateDto dto)
    {
        var course = await _courseRepository.GetByIdAsync(courseId);
        if (course is null)
        {
            return NotFound(new ApiResponseDto<LessonResponseDto>
            {
                Success = false,
                Message = "Curso no encontrado"
            });
        }

        var existingLessons = await _lessonRepository.GetByCourseAsync(courseId);
        var nextOrder = existingLessons.Any() ? existingLessons.Max(l => l.Order) + 1 : 1;

        var lesson = new Lesson
        {
            Id = Guid.NewGuid(),
            CourseId = courseId,
            Title = dto.Title,
            Order = nextOrder,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _lessonRepository.AddAsync(lesson);

        var response = new LessonResponseDto
        {
            Id = lesson.Id,
            Title = lesson.Title,
            Order = lesson.Order,
            CreatedAt = lesson.CreatedAt,
            UpdatedAt = lesson.UpdatedAt
        };

        return CreatedAtAction(nameof(GetByCourse), new { courseId }, new ApiResponseDto<LessonResponseDto>
        {
            Success = true,
            Message = "Lección creada correctamente",
            Data = response
        });
    }

    // PUT: api/courses/{courseId}/lessons/{id}
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponseDto<LessonResponseDto>>> Update(Guid courseId, Guid id, [FromBody] LessonUpdateDto dto)
    {
        var lesson = await _lessonRepository.GetByIdAsync(id);

        if (lesson is null || lesson.CourseId != courseId)
        {
            return NotFound(new ApiResponseDto<LessonResponseDto>
            {
                Success = false,
                Message = "Lección no encontrada"
            });
        }

        lesson.Title = dto.Title;
        lesson.UpdatedAt = DateTime.UtcNow;

        await _lessonRepository.UpdateAsync(lesson);

        var response = new LessonResponseDto
        {
            Id = lesson.Id,
            Title = lesson.Title,
            Order = lesson.Order,
            CreatedAt = lesson.CreatedAt,
            UpdatedAt = lesson.UpdatedAt
        };

        return Ok(new ApiResponseDto<LessonResponseDto>
        {
            Success = true,
            Message = "Lección actualizada correctamente",
            Data = response
        });
    }

    // DELETE: api/courses/{courseId}/lessons/{id}
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponseDto<object>>> Delete(Guid courseId, Guid id)
    {
        var lesson = await _lessonRepository.GetByIdAsync(id);

        if (lesson is null || lesson.CourseId != courseId)
        {
            return NotFound(new ApiResponseDto<object>
            {
                Success = false,
                Message = "Lección no encontrada"
            });
        }

        await _lessonRepository.SoftDeleteAsync(id);

        return Ok(new ApiResponseDto<object>
        {
            Success = true,
            Message = "Lección eliminada correctamente"
        });
    }

    // PATCH: api/courses/{courseId}/lessons/{id}/move-up
    [HttpPatch("{id:guid}/move-up")]
    public async Task<ActionResult<ApiResponseDto<object>>> MoveUp(Guid courseId, Guid id)
    {
        var lesson = await _lessonRepository.GetByIdAsync(id);

        if (lesson is null || lesson.CourseId != courseId)
        {
            return NotFound(new ApiResponseDto<object>
            {
                Success = false,
                Message = "Lección no encontrada"
            });
        }

        await _lessonRepository.MoveUpAsync(id);

        return Ok(new ApiResponseDto<object>
        {
            Success = true,
            Message = "Lección reordenada (subir)"
        });
    }

    // PATCH: api/courses/{courseId}/lessons/{id}/move-down
    [HttpPatch("{id:guid}/move-down")]
    public async Task<ActionResult<ApiResponseDto<object>>> MoveDown(Guid courseId, Guid id)
    {
        var lesson = await _lessonRepository.GetByIdAsync(id);

        if (lesson is null || lesson.CourseId != courseId)
        {
            return NotFound(new ApiResponseDto<object>
            {
                Success = false,
                Message = "Lección no encontrada"
            });
        }

        await _lessonRepository.MoveDownAsync(id);

        return Ok(new ApiResponseDto<object>
        {
            Success = true,
            Message = "Lección reordenada (bajar)"
        });
    }
}
