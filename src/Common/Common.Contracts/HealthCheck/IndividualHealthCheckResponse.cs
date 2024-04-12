namespace Common.Contracts.HealthCheck;

public record IndividualHealthCheckResponse(
	string? Status,
	string? Component,
	string? Description,
	string? ExceptionMessage);