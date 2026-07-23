using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/archive")]
public class ArchiveController(TmsDbContext context) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> ArchiveOldEnrollments()
    {
        var cutoff = DateTime.UtcNow.AddYears(-1);

        var updated = await context.Enrollments
            .Where(e => e.EnrolledAt < cutoff)
            .ExecuteUpdateAsync(s =>
                s.SetProperty(e => e.IsArchived, true));

        return Ok(new
        {
            UpdatedRows = updated
        });
    }
}