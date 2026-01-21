using System.Net;
using System.Net.Http.Json;

namespace Common.Presentation.Http;

public static class HttpResult
{
    public static async Task<Result<T>> ReadJsonAsResultAsync<T>(
        HttpResponseMessage response,
        CancellationToken ct)
    {
        if (!response.IsSuccessStatusCode)
        {
            return Result<T>.Fail(MapProblem(response.StatusCode));
        }

        var value = await response.Content.ReadFromJsonAsync<T>(cancellationToken: ct);

        return value is null
            ? Result<T>.Fail(Problems.BadGateway("Upstream returned an empty JSON body."))
            : Result<T>.Ok(value);
    }

    public static async Task<Result<string>> ReadStringAsResultAsync(
        HttpResponseMessage response,
        CancellationToken ct)
    {
        if (!response.IsSuccessStatusCode)
        {
            return Result<string>.Fail(MapProblem(response.StatusCode));
        }

        var value = await response.Content.ReadAsStringAsync(ct);

        return string.IsNullOrWhiteSpace(value)
            ? Result<string>.Fail(Problems.BadGateway("Upstream returned an empty response body."))
            : Result<string>.Ok(value);
    }

    private static Problem MapProblem(HttpStatusCode statusCode)
    {
        return statusCode switch
        {
            HttpStatusCode.NotFound =>
                Problems.NotFound("Upstream resource not found."),

            HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden =>
                Problems.ServiceUnavailable(
                    "Upstream authentication failed."), // or Problems.Unauthorized if you want to expose it

            (HttpStatusCode)429 =>
                Problems.ServiceUnavailable("Upstream rate limit reached."),

            HttpStatusCode.BadRequest =>
                Problems.BadGateway("Bad request to upstream service."),

            _ when (int)statusCode >= 500 =>
                Problems.ServiceUnavailable("Upstream service unavailable."),

            _ =>
                Problems.BadGateway($"Unexpected upstream response (HTTP {(int)statusCode}).")
        };
    }
}