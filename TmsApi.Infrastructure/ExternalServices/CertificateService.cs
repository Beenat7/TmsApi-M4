using System.Net.Http.Json;
using Polly;
using Polly.Registry;
using TmsApi.Application.Interfaces;
using Microsoft.Extensions.Logging;
namespace TmsApi.Infrastructure.ExternalServices;

public class CertificateService(
    ResiliencePipelineProvider<string> pipelineProvider,
    HttpClient httpClient,
    ILogger<CertificateService> logger)
    : ICertificateService
{
    public async Task<CertificateResult> IssueCertificateAsync(
        int studentId,
        string courseCode,
        CancellationToken ct)
    {
        var pipeline = pipelineProvider.GetPipeline("certificate-api");

        return await pipeline.ExecuteAsync(async token =>
        {
            logger.LogInformation(
                "Requesting certificate for student {StudentId}, course {CourseCode}",
                studentId,
                courseCode);

            using var response = await httpClient.PostAsJsonAsync(
                "/fake/certificates",
                new
                {
                    StudentId = studentId,
                    CourseCode = courseCode
                },
                token);

            // 5xx = transient failure.
            // Throw so Polly can retry/break the circuit.
            if ((int)response.StatusCode >= 500)
            {
                throw new HttpRequestException(
                    $"Certificate service returned {(int)response.StatusCode}.");
            }

            // 4xx = request-specific failure.
            // Do NOT throw HttpRequestException here because
            // Polly would treat that as transient and retry it.
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(token);

                throw new InvalidOperationException(
                    $"Certificate service rejected request: " +
                    $"{(int)response.StatusCode} {error}");
            }

            return await response.Content
                .ReadFromJsonAsync<CertificateResult>(
                    cancellationToken: token)
                ?? throw new InvalidOperationException(
                    "Certificate service returned an empty response.");
        }, ct);
    }
}