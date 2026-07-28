using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Infrastructure.Persistence;
using TmsApi.Application.Interfaces;

namespace TmsApi.Api.Controllers.V2;

[ApiController]
[Route("api/v{version:apiVersion}/courses")]
[ApiVersion("2.0")]
public class CoursesController(
    ICachedCourseService cachedCourseService)
    : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetCourses(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var courses = await cachedCourseService.GetAllCoursesAsync(ct);
        var totalCount = courses.Count;
        var rows = courses
            .OrderBy(c => c.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var hasNext = page < totalPages;
        var hasPrevious = page > 1;

        return Ok(new
        {
            data = rows,
            meta = new
            {
                totalCount,
                page,
                pageSize,
                totalPages,
                hasNext,
                hasPrevious
            },
            links = new
            {
                self = $"/api/v2/courses?page={page}&pageSize={pageSize}",
                next = hasNext
                    ? $"/api/v2/courses?page={page + 1}&pageSize={pageSize}"
                    : (string?)null,
                prev = hasPrevious
                    ? $"/api/v2/courses?page={page - 1}&pageSize={pageSize}"
                    : (string?)null,
                enroll = "/api/v2/enrollments"
            }
        });
    }
}