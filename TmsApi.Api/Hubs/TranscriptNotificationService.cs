using Microsoft.AspNetCore.SignalR;
using TmsApi.Application.Hubs;

namespace TmsApi.Api.Hubs;

public class TranscriptNotificationService(
    IHubContext<TmsHub, ITmsHubClient> hubContext)
    : ITranscriptNotificationService
{
    public Task NotifyTranscriptReadyAsync(
        int studentId,
        string reportId,
        string downloadUrl,
        CancellationToken ct = default)
    {
        return hubContext.Clients
            .Group(GroupNames.Student(studentId.ToString()))
            .ReceiveTranscriptReady(
                reportId,
                downloadUrl);
    }
}