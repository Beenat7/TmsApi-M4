namespace TmsApi.Application.Authentication;

public record UserProfileDto(
    string DisplayName,
    string Role);