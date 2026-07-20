using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Interfaces;
using TmsApi.Data;
using TmsApi.Entities;

namespace TmsApi.Infrastructure.Repositories;

public class CourseRepository(TmsDbContext context)
    : ICourseRepository
{
    public async Task<Course?> GetByCodeAsync(
        string courseCode,
        CancellationToken ct)
    {
        return await context.Courses
            .Include(c => c.Enrollments)
            .FirstOrDefaultAsync(
                c => c.Code == courseCode,
                ct);
    }

    public async Task<Course?> GetByIdAsync(
        int id,
        CancellationToken ct)
    {
        return await context.Courses
            .Include(c => c.Enrollments)
            .FirstOrDefaultAsync(
                c => c.Id == id,
                ct);
    }
}