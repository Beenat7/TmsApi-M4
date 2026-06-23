using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/enrollments")]
public class EnrollmentsController : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService;

    public EnrollmentsController(IEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }

    // GET: /api/enrollments
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _enrollmentService.GetAllAsync();
        return Ok(result);
    }

    // GET: /api/enrollments/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _enrollmentService.GetByIdAsync(id);

        return result is null
            ? NotFound()
            : Ok(result);
    }

    // POST: /api/enrollments
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEnrollmentRequest request)
    {
        var result = await _enrollmentService.EnrollAsync(
            request.StudentId,
            request.CourseCode);

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            result);
    }

    // DELETE: /api/enrollments/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await _enrollmentService.DeleteAsync(id);

        return deleted ? NoContent() : NotFound();
    }
}

public record CreateEnrollmentRequest(string StudentId, string CourseCode);

