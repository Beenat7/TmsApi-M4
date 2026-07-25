using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Infrastructure.Persistence.Repositories;

public class CourseRepository(TmsDbContext context) : ICourseRepository
{
    public async Task<Course?> GetByCodeAsync(
        string courseCode,
        CancellationToken ct = default)
    {
        return await context.Courses
            .Include(c => c.Enrollments)
            .FirstOrDefaultAsync(
                c => c.Code == courseCode,
                ct);
    }
}