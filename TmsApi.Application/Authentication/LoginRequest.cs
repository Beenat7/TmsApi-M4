namespace TmsApi.Application.Authentication;

public record LoginRequest(
    string Username,
    string Password);