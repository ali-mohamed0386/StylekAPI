namespace Se7ety.Api.DTOs.Common;

public sealed record HealthCheckResponse(
    string Status,
    string Environment,
    DateTime CheckedAtUtc);

public sealed record DatabaseHealthCheckResponse(
    string Status,
    bool CanConnect,
    string Environment,
    DateTime CheckedAtUtc,
    string? Error = null);
