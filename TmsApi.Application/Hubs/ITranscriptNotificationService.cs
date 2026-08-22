namespace TmsApi.Application.Hubs;

public interface ITranscriptNotificationService
{
    Task NotifyTranscriptReadyAsync(
        int studentId,
        string reportId,
        string downloadUrl,
        CancellationToken ct = default);
}