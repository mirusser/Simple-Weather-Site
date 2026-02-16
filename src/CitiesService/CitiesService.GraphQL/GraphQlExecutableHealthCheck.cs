using System.Text;
using HotChocolate.Execution;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace CitiesService.GraphQL;

public sealed class GraphQlExecutableHealthCheck(
    IRequestExecutorResolver executorResolver,
    ILogger<GraphQlExecutableHealthCheck> logger) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken ct = default)
    {
        try
        {
            var executor = await executorResolver.GetRequestExecutorAsync(cancellationToken: ct);

            IExecutionResult exec = await executor.ExecuteAsync("{ ping }", ct);

            if (exec is IOperationResult op)
            {
                var errors = op.Errors;
                if (errors is { Count: > 0 })
                {
                    var sb = new StringBuilder();
                    foreach (var e in errors)
                        sb.AppendLine(e.Message);

                    return HealthCheckResult.Unhealthy(
                        "GraphQL execution returned errors.",
                        data: new Dictionary<string, object?> { ["errors"] = sb.ToString() }!);
                }

                return HealthCheckResult.Healthy("GraphQL schema is executable.");
            }

            return HealthCheckResult.Unhealthy(
                "GraphQL execution did not return an operation result.",
                data: new Dictionary<string, object?> { ["resultType"] = exec.GetType().FullName }!);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occured while executing the GraphQL query.");
            return HealthCheckResult.Unhealthy("GraphQL executor failed.", ex);
        }
    }
}