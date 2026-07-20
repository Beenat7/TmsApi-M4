using TmsApi.Entities;
namespace TmsApi.Application.Interfaces;

public interface ICourseRepository
{
    Task<Course?> GetByCodeAsync(
        string courseCode,
        CancellationToken ct);

    Task<Course?> GetByIdAsync(
        int id,
        CancellationToken ct);
}