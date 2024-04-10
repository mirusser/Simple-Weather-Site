namespace Common.Contracts.HealthCheck;

public record HealthCheckResponse(
	string? Status,
	IEnumerable<IndividualHealthCheckResponse>? HealthChecks,
	TimeSpan HealthCheckDuration);