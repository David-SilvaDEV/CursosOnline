using CursosOnline.Application.DTOs.Common;
using CursosOnline.Application.DTOs.CursosOnline;
using CursosOnline.Domain.Entities;
using CursosOnline.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CursosOnline.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class CoursesController : ControllerBase
{
    private readonly ICourseRepository _courseRepository;
    private readonly ILessonRepository _lessonRepository;

    public CoursesController(ICourseRepository courseRepository, ILessonRepository lessonRepository)
    {
        _courseRepository = courseRepository;
        _lessonRepository = lessonRepository;
    }
    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<CourseListDto>>>> GetAll()
    {
        var courses = await _courseRepository.GetAllAsync();

        var list = courses.Select(c => new CourseListDto
        {
            Id = c.Id,
            Title = c.Title,
            Status = c.Status.ToString()
        });

        return Ok(new ApiResponseDto<IEnumerable<CourseListDto>>
        {
            Success = true,
            Message = "Listado de cursos",
            Data = list
        });
    }

    // GET: api/courses/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponseDto<CourseResponseDto>>> GetById(Guid id)
    {
        var course = await _courseRepository.GetByIdAsync(id);

        if (course is null)
        {
            return NotFound(new ApiResponseDto<CourseResponseDto>
            {
                Success = false,
                Message = "Curso no encontrado"
            });
        }

        var dto = new CourseResponseDto
        {
            Id = course.Id,
            Title = course.Title,
            Status = course.Status.ToString(),
            CreatedAt = course.CreatedAt,
            UpdatedAt = course.UpdatedAt
        };

        return Ok(new ApiResponseDto<CourseResponseDto>
        {
            Success = true,
            Message = "Detalle de curso",
            Data = dto
        });
    }

    // POST: api/courses
    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<CourseResponseDto>>> Create([FromBody] CourseCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponseDto<CourseResponseDto>
            {
                Success = false,
                Message = "Datos inválidos"
            });
        }

        var course = new Course
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Status = CourseStatus.Draft,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _courseRepository.AddAsync(course);

        var responseDto = new CourseResponseDto
        {
            Id = course.Id,
            Title = course.Title,
            Status = course.Status.ToString(),
            CreatedAt = course.CreatedAt,
            UpdatedAt = course.UpdatedAt
        };

        return CreatedAtAction(nameof(GetById), new { id = course.Id }, new ApiResponseDto<CourseResponseDto>
        {
            Success = true,
            Message = "Curso creado correctamente",
            Data = responseDto
        });
    }

    // PUT: api/courses/{id}
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponseDto<CourseResponseDto>>> Update(Guid id, [FromBody] CourseUpdateDto dto)
    {
        var course = await _courseRepository.GetByIdAsync(id);

        if (course is null)
        {
            return NotFound(new ApiResponseDto<CourseResponseDto>
            {
                Success = false,
                Message = "Curso no encontrado"
            });
        }

        course.Title = dto.Title;

        if (Enum.TryParse<CourseStatus>(dto.Status, out var status))
        {
            course.Status = status;
        }

        course.UpdatedAt = DateTime.UtcNow;

        await _courseRepository.UpdateAsync(course);

        var responseDto = new CourseResponseDto
        {
            Id = course.Id,
            Title = course.Title,
            Status = course.Status.ToString(),
            CreatedAt = course.CreatedAt,
            UpdatedAt = course.UpdatedAt
        };

        return Ok(new ApiResponseDto<CourseResponseDto>
        {
            Success = true,
            Message = "Curso actualizado correctamente",
            Data = responseDto
        });
    }

    // DELETE: api/courses/{id}
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponseDto<object>>> Delete(Guid id)
    {
        var course = await _courseRepository.GetByIdAsync(id);

        if (course is null)
        {
            return NotFound(new ApiResponseDto<object>
            {
                Success = false,
                Message = "Curso no encontrado"
            });
        }

        await _courseRepository.SoftDeleteAsync(id);

        return Ok(new ApiResponseDto<object>
        {
            Success = true,
            Message = "Curso eliminado correctamente"
        });
    }

    // PATCH: api/courses/{id}/publish
    [HttpPatch("{id:guid}/publish")]
    public async Task<ActionResult<ApiResponseDto<object>>> Publish(Guid id)
    {
        var course = await _courseRepository.GetByIdAsync(id);
        if (course is null)
        {
            return NotFound(new ApiResponseDto<object>
            {
                Success = false,
                Message = "Curso no encontrado"
            });
        }

        var lessons = await _lessonRepository.GetByCourseAsync(id);
        if (!lessons.Any())
        {
            return BadRequest(new ApiResponseDto<object>
            {
                Success = false,
                Message = "Un curso solo puede publicarse si tiene al menos una lección activa"
            });
        }

        course.Status = CourseStatus.Published;
        course.UpdatedAt = DateTime.UtcNow;
        await _courseRepository.UpdateAsync(course);

        return Ok(new ApiResponseDto<object>
        {
            Success = true,
            Message = "Curso publicado correctamente"
        });
    }

    // PATCH: api/courses/{id}/unpublish
    [HttpPatch("{id:guid}/unpublish")]
    public async Task<ActionResult<ApiResponseDto<object>>> Unpublish(Guid id)
    {
        var course = await _courseRepository.GetByIdAsync(id);
        if (course is null)
        {
            return NotFound(new ApiResponseDto<object>
            {
                Success = false,
                Message = "Curso no encontrado"
            });
        }

        course.Status = CourseStatus.Draft;
        course.UpdatedAt = DateTime.UtcNow;
        await _courseRepository.UpdateAsync(course);

        return Ok(new ApiResponseDto<object>
        {
            Success = true,
            Message = "Curso despublicado correctamente"
        });
    }

    // GET: api/courses/search?q=&status=&page=&pageSize=
    [HttpGet("search")]
    public async Task<ActionResult<PagedResponseDto<CourseListDto>>> Search(
        [FromQuery] string? q,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 10;

        CourseStatus? parsedStatus = null;
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<CourseStatus>(status, true, out var s))
        {
            parsedStatus = s;
        }

        var (items, totalCount) = await _courseRepository.SearchAsync(q, parsedStatus, page, pageSize);

        var dtoItems = items.Select(c => new CourseListDto
        {
            Id = c.Id,
            Title = c.Title,
            Status = c.Status.ToString()
        });

        return Ok(new PagedResponseDto<CourseListDto>
        {
            Success = true,
            Message = "Resultados de búsqueda",
            Data = dtoItems,
            PageNumber = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    // GET: api/courses/{id}/summary
    [HttpGet("{id:guid}/summary")]
    public async Task<ActionResult<ApiResponseDto<CourseSummaryDto>>> Summary(Guid id)
    {
        var course = await _courseRepository.GetByIdAsync(id);
        if (course is null)
        {
            return NotFound(new ApiResponseDto<CourseSummaryDto>
            {
                Success = false,
                Message = "Curso no encontrado"
            });
        }

        var lessons = await _lessonRepository.GetByCourseAsync(id);
        var totalLessons = lessons.Count();
        var lastLessonUpdate = lessons.Any()
            ? lessons.Max(l => l.UpdatedAt)
            : (DateTime?)null;

        var lastModified = lastLessonUpdate.HasValue
            ? (lastLessonUpdate.Value > course.UpdatedAt ? lastLessonUpdate.Value : course.UpdatedAt)
            : course.UpdatedAt;

        var dto = new CourseSummaryDto
        {
            Id = course.Id,
            Title = course.Title,
            Status = course.Status.ToString(),
            TotalLessons = totalLessons,
            LastModifiedAt = lastModified
        };

        return Ok(new ApiResponseDto<CourseSummaryDto>
        {
            Success = true,
            Message = "Resumen de curso",
            Data = dto
        });
    }
}
