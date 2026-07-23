using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/registrar")]
public class RegistrarController(TmsDbContext context) : ControllerBase
{
 
    [HttpGet("active-high-gpa-count")]
public async Task<IActionResult> GetActiveHighGpaCount()
{
    var count = await context.Students
        .Where(s => s.IsActive && s.GPA >= 3.0m)
        .CountAsync();

    return Ok(count);
}


[HttpGet("courses-by-enrollment")]
public async Task<IActionResult> GetCoursesByEnrollment()
{
    var list = await context.Courses
        .Select(c => new
        {
            c.Title,
            EnrollmentCount = c.Enrollments.Count
        })
        .OrderByDescending(x => x.EnrollmentCount)
        .ToListAsync();

    return Ok(list);
}


[HttpGet("average-gpa-per-course")]
public async Task<IActionResult> GetAverageGpaPerCourse()
{
    var list = await context.Enrollments
        .GroupBy(e => e.Course.Title)
        .Select(g => new
        {
            Course = g.Key,
            AverageGPA = g.Average(e => e.Student.GPA)
        })
        .ToListAsync();

    return Ok(list);
}


[HttpGet("students-with-no-enrollments")]
public async Task<IActionResult> GetStudentsWithNoEnrollments()
{
    var list = await context.Students
        .Where(s => !s.Enrollments.Any())
        .Select(s => s.Name)
        .ToListAsync();

    return Ok(list);
}


[HttpGet("students-with-no-enrollments-join")]
public async Task<IActionResult> GetStudentsWithNoEnrollmentsJoin()
{
    var list = await context.Students
        .GroupJoin(
            context.Enrollments,
            s => s.Id,
            e => e.StudentId,
            (s, e) => new { s, e }
        )
        .SelectMany(
            x => x.e.DefaultIfEmpty(),
            (x, e) => new { x.s, e }
        )
        .Where(x => x.e == null)
        .Select(x => x.s.Name)
        .ToListAsync();

    return Ok(list);
}


    


}