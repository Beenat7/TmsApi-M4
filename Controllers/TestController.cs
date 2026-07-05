using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using TmsApi.Data;
namespace TmsApi.Controllers;
using Microsoft.EntityFrameworkCore;
using TmsApi.Entities;
[ApiController]
[Route("api/test")]
public class TestController(TmsDbContext context) : ControllerBase
{
[HttpGet("deferred")]
public IActionResult TestDeferred()
{
Console.WriteLine("\n>>> STEP 1: Building the query object (nodatabase contact)...");
var query = context.Students.Where(s => s.GPA >= 3.0m);
Console.WriteLine(">>> STEP 2: Appending a sorting clause...");var orderedQuery = query.OrderBy(s => s.Name);
Console.WriteLine(">>> STEP 3: Materializing query into a C#List...");
var results = orderedQuery.ToList(); // Execution is triggeredhere
Console.WriteLine(">>> STEP 4: Materialization finished. Listpopulated.\n");
return Ok(results);
}



// Non-translatable helper method
private static bool IsHonorRoll(decimal gpa)
{
return gpa >= 3.5m;
}
[HttpGet("translation-fail")]
public IActionResult TestTranslationFail()
{
Console.WriteLine("\n>>> STEP 1: Running non-translatable query...");
try
{
var students = context.Students
.Where(s => IsHonorRoll(s.GPA)) // EF Core does not know how to map this method to SQL
.ToList();
return Ok(students);
}
catch (Exception ex)
{
Console.WriteLine($">>> EXCEPTION CAUGHT: {ex.Message}\n");return BadRequest(new { Message = ex.Message });
}
}



[HttpGet("nplusone")]
public async Task<IActionResult> NPlusOne()
{
    var students = await context.Students
        .AsNoTracking()
        .ToListAsync();

    foreach (var s in students)
    {
        var count = await context.Enrollments
            .AsNoTracking()
            .CountAsync(e => e.StudentId == s.Id);

        Console.WriteLine($"{s.Name}: {count} enrollments");
    }

    return Ok("Finished");
}



[HttpGet("nplusone-fixed")]
public async Task<IActionResult> NPlusOneFixed()
{
    var report = await context.Students
        .AsNoTracking()
        .Select(s => new
        {
            s.Name,
            EnrollmentCount = s.Enrollments.Count
        })
        .ToListAsync();

    foreach (var r in report)
    {
        Console.WriteLine($"{r.Name}: {r.EnrollmentCount} enrollments");
    }

    return Ok(report);
}


}




