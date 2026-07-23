using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/analytics")]
public class AnalyticsController(TmsDbContext context) : ControllerBase
{
    [HttpGet("students")]
    public async Task<IActionResult> GetStudents(
        int page = 1,
        int pageSize = 20)
    {
        var students = await context.Students
            .OrderBy(s => s.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(students);
    }


    [HttpGet("top-courses")]
public async Task<IActionResult> GetTopCourses()
{
    var result = await context.Courses
        .Select(c => new
        {
            c.Title,
            EnrollmentCount = c.Enrollments.Count
        })
        .OrderByDescending(x => x.EnrollmentCount)
        .Take(5)
        .ToListAsync();

    return Ok(result);
}




}