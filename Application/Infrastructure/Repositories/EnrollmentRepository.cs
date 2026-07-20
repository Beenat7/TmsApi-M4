using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Interfaces;
using TmsApi.Data;
using TmsApi.Entities;

namespace TmsApi.Infrastructure.Repositories;

public class EnrollmentRepository(TmsDbContext context)
    : IEnrollmentRepository
{
    public async Task<bool> ExistsAsync(
        int studentId,
        string courseCode,
        CancellationToken ct)
    {
        return await context.Enrollments
            .Include(e => e.Course)
            .AnyAsync(
                e => e.StudentId == studentId &&
                     e.Course.Code == courseCode,
                ct);
    }

    public async Task AddAsync(
        Enrollment enrollment,
        CancellationToken ct)
    {
        context.Enrollments.Add(enrollment);
        await context.SaveChangesAsync(ct);
    }

    public async Task<List<Enrollment>> GetByStudentIdAsync(
        int studentId,
        CancellationToken ct)
    {
        return await context.Enrollments
            .Include(e => e.Course)
            .Where(e => e.StudentId == studentId)
            .ToListAsync(ct);
    }
}