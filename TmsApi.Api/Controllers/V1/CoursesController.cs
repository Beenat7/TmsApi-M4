using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Infrastructure.Persistence;

using Microsoft.AspNetCore.Authorization;
using TmsApi.Api.Authorization;
using TmsApi.Domain.Entities;

namespace TmsApi.Api.Controllers.V1;

[ApiController]
[Route("api/v{version:apiVersion}/courses")]
[ApiVersion("1.0")]
public class CoursesController(
    TmsDbContext context,
    IAuthorizationService authorizationService)
    : ControllerBase
{
    public record UpdateCourseRequest(
    string Title,
    int MaxCapacity);

    [HttpGet]
    public async Task<IActionResult> GetCourses(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var baseQuery = context.Courses.AsNoTracking();
        var totalCount = await baseQuery.CountAsync(ct);
        var items = await baseQuery
            .OrderBy(c => c.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new
            {
                c.Id,
                c.Code,
                c.Title,
                c.MaxCapacity,
                EnrollmentCount = c.Enrollments.Count
            })
            .ToListAsync(ct);

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return Ok(new
        {
            items,
            totalCount,
            page,
            pageSize,
            totalPages,
            hasNext = page < totalPages,
            hasPrevious = page > 1
        });
    }

   
         [Authorize(Roles = "Instructor,Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCourse(
            int id,
            [FromBody] UpdateCourseRequest request,
            CancellationToken ct = default)
        {
            var course = await context.Courses
                .FirstOrDefaultAsync(c => c.Id == id, ct);

            if (course is null)
            {
                return NotFound(new
                {
                    detail = $"Course {id} was not found."
                });
            }

            var authorizationResult =
                await authorizationService.AuthorizeAsync(
                    User,
                    course,
                    "CanEditCourse");

            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            course.Title = request.Title;
            course.MaxCapacity = request.MaxCapacity;

            await context.SaveChangesAsync(ct);

            return NoContent();
        }



    
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCourse(
        int id,
        CancellationToken ct = default)
    {
        var course = await context.Courses
            .Include(c => c.Enrollments)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        if (course is null)
        {
            return NotFound(new
            {
                detail = $"Course {id} was not found."
            });
        }

        if (course.Enrollments.Any())
        {
            return Conflict(new
            {
                detail = $"Course {course.Code} cannot be deleted because it has active enrollments."
            });
        }

        context.Courses.Remove(course);
        await context.SaveChangesAsync(ct);
        return NoContent();
    }


}