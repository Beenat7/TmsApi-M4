using TmsApi.Application.DTOs;

namespace TmsApi.Application.Interfaces;

public interface ICachedCourseService
{
    Task<CourseResponseDto> GetCourseAsync(
        string code,
        CancellationToken ct = default);
    Task<List<CourseResponseDto>> GetAllCoursesAsync(
        CancellationToken ct = default);
    Task InvalidateCourseCacheAsync(
        CancellationToken ct = default);
}