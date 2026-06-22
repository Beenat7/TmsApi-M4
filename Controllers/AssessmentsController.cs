using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AssessmentsController : ControllerBase
{
    [Authorize]
    [HttpGet("results")]
    public IActionResult GetResults()
    {
        return Ok(new
        {
            courseCode = "CS-101",
            studentId = "S-001",
            letterGrade = "A"
        });
    }
}