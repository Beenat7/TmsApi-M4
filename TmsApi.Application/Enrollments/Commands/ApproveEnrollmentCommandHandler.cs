using MediatR;
using TmsApi.Application.Common;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Enums;

namespace TmsApi.Application.Enrollments.Commands;

public class ApproveEnrollmentHandler(
    IEnrollmentRepository enrollmentRepo)
    : IRequestHandler<
        ApproveEnrollmentCommand,
        Result<EnrollmentApproved, EnrollmentApprovalError>>
{
    public async Task<
        Result<EnrollmentApproved, EnrollmentApprovalError>> Handle(
            ApproveEnrollmentCommand command,
            CancellationToken ct)
    {
        var enrollment =
            await enrollmentRepo.GetByIdAsync(
                command.EnrollmentId,
                ct);

        if (enrollment is null)
        {
            return Result<
                EnrollmentApproved,
                EnrollmentApprovalError>.Failure(
                    EnrollmentApprovalError.NotFound(
                        command.EnrollmentId));
        }

        if (enrollment.IsArchived)
        {
            return Result<
                EnrollmentApproved,
                EnrollmentApprovalError>.Failure(
                    EnrollmentApprovalError.Archived(
                        command.EnrollmentId));
        }

        if (enrollment.Status == EnrollmentStatus.Approved)
        {
            return Result<
                EnrollmentApproved,
                EnrollmentApprovalError>.Failure(
                    EnrollmentApprovalError.AlreadyApproved(
                        command.EnrollmentId));
        }

        enrollment.Status = EnrollmentStatus.Approved;

        await enrollmentRepo.SaveChangesAsync(ct);

        return Result<
            EnrollmentApproved,
            EnrollmentApprovalError>.Success(
                new EnrollmentApproved(enrollment.Id));
    }
}