using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using TmsApi.Domain.Entities;

namespace TmsApi.Api.Authorization;

public sealed class CourseInstructorHandler
    : AuthorizationHandler<CourseInstructorRequirement, Course>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CourseInstructorRequirement requirement,
        Course resource)
    {
        var userId =
            context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        var isInstructor =
            context.User.IsInRole("Instructor");

        var isAdmin =
            context.User.IsInRole("Admin");

        // Administrators can manage any course.
        if (isAdmin)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // Instructors can only manage courses assigned to them.
        if (isInstructor &&
            resource.InstructorId == userId)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}