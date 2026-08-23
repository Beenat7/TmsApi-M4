using MediatR;
using TmsApi.Application.Common;

namespace TmsApi.Application.Enrollments.Commands;

public record ApproveEnrollmentCommand(
    int EnrollmentId)
    : IRequest<Result<EnrollmentApproved, EnrollmentApprovalError>>;

public record EnrollmentApproved(
    int EnrollmentId);

public record EnrollmentApprovalError(
    string Code,
    string Message)
{
    public static EnrollmentApprovalError NotFound(
        int enrollmentId) =>
        new(
            "enrollment_not_found",
            $"Enrollment {enrollmentId} was not found.");

    public static EnrollmentApprovalError AlreadyApproved(
        int enrollmentId) =>
        new(
            "already_approved",
            $"Enrollment {enrollmentId} is already approved.");

    public static EnrollmentApprovalError Archived(
        int enrollmentId) =>
        new(
            "enrollment_archived",
            $"Enrollment {enrollmentId} is archived.");
}