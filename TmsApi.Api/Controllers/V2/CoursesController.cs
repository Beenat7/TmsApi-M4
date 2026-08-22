using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;
using TmsApi.Application.Utilities;

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
        [FromQuery] string? fields,
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

        var totalPages = (int)Math.Ceiling(
            totalCount / (double)pageSize);

        var hasNext = page < totalPages;
        var hasPrevious = page > 1;

        var courseDtos = rows.Select(c => new CourseResponseDto(
            c.Id,
            c.Code,
            c.Title,
            c.MaxCapacity,
            c.EnrollmentCount));

        var shaped = courseDtos.ShapeData(
            fields,
            CourseResponseDtoFields.Allowed);

        var links = new List<LinkDto>
        {
            new(
                Url.Action(
                    nameof(GetCourses),
                    new
                    {
                        page,
                        pageSize,
                        fields
                    })!,
                "self",
                "GET")
        };

        if (hasNext)
        {
            links.Add(
                new LinkDto(
                    Url.Action(
                        nameof(GetCourses),
                        new
                        {
                            page = page + 1,
                            pageSize,
                            fields
                        })!,
                    "next",
                    "GET"));
        }

        if (hasPrevious)
        {
            links.Add(
                new LinkDto(
                    Url.Action(
                        nameof(GetCourses),
                        new
                        {
                            page = page - 1,
                            pageSize,
                            fields
                        })!,
                    "prev",
                    "GET"));
        }

        return Ok(new
        {
            data = shaped,
            meta = new
            {
                totalCount,
                page,
                pageSize,
                totalPages,
                hasNext,
                hasPrevious
            },
            links
        });
    }
}