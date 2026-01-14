using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Common.ExtensionMethods;
using Common.Infrastructure.Managers.Contracts;
using Common.Infrastructure.Settings;
using Common.Presentation.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Polly.Registry;
using WeatherService.Clients.Responses;
using WeatherService.Models;
using WeatherService.Settings;
using Clouds = WeatherService.Clients.Responses.Clouds;
using Coord = WeatherService.Clients.Responses.Coord;
using Weather = WeatherService.Clients.Responses.Weather;
using Wind = WeatherService.Clients.Responses.Wind;

namespace WeatherService.Clients;

public sealed class WeatherClient(
    IHttpExecutor httpExecutor,
    IOptions<ServiceSettings> options)
{
    private const string ClientName = "OpenWeather";
    private const string CurrentWeatherPath = "data/2.5/weather";
    private readonly ServiceSettings settings = options.Value;

    public async Task<Result<Forecast>> GetCurrentWeatherByCityNameAsync(string city, CancellationToken ct)
    {
        var uri = BuildWeatherUri(city: city, cityId: null, mode: null);

        using var req = new HttpRequestMessage(HttpMethod.Get, uri);
        using var res = await httpExecutor.SendAsync(ClientName, req, ct);

        return await HttpResult.ReadJsonAsResultAsync<Forecast>(res, ct);
    }

    public async Task<Result<Forecast>> GetCurrentWeatherByCityIdAsync(
        int cityId,
        CancellationToken ct)
    {
        var uri = BuildWeatherUri(city: null, cityId: cityId, mode: null);

        using var req = new HttpRequestMessage(HttpMethod.Get, uri);
        using var res = await httpExecutor.SendAsync(ClientName, req, ct);

        return await HttpResult.ReadJsonAsResultAsync<Forecast>(res, ct);
    }

    public async Task<Result<Current>> GetCurrentXmlByCityAsync(string city, CancellationToken ct)
    {
        var uri = BuildWeatherUri(city: city, cityId: null, mode: "xml");

        using var req = new HttpRequestMessage(HttpMethod.Get, uri);
        using var res = await httpExecutor.SendAsync(ClientName, req, ct);

        var xmlResult = await HttpResult.ReadStringAsResultAsync(res, ct);
        if (!xmlResult.IsSuccess)
        {
            return Result<Current>.Fail(xmlResult.Problem!);
        }

        try
        {
            var current = xmlResult.Value!.DeserializeXml<Current>();
            return Result<Current>.Ok(current);
        }
        catch (InvalidOperationException ex) // XmlSerializer throws this on invalid XML
        {
            // "upstream returned junk"
            return Result<Current>.Fail(Problems.BadGateway($"Invalid XML received from upstream: {ex.Message}"));
        }
    }

    private Uri BuildWeatherUri(string? city, int? cityId, string? mode)
    {
        if (!string.IsNullOrWhiteSpace(city) == (cityId is not null))
        {
            throw new ArgumentException("Provide either city or cityId, but not both.");
        }

        // Base required query parameters
        var query = new Dictionary<string, string?>
        {
            ["appid"] = settings.ApiKey,
            ["units"] = "metric",
        };

        // Choose exactly one identifier
        if (!string.IsNullOrWhiteSpace(city))
        {
            query["q"] = city;
        }

        if (cityId is not null)
        {
            query["id"] = cityId.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (!string.IsNullOrWhiteSpace(mode))
        {
            query["mode"] = mode;
        }

        var relative = QueryHelpers.AddQueryString(CurrentWeatherPath, query);
        return new Uri(relative, UriKind.Relative);
    }
}

public class WeatherClient123412341234(
    IHttpClientFactory clientFactory,
    ResiliencePipelineProvider<string> pipelineProvider,
    IOptions<ResiliencePipeline> resiliencePipelineOptions,
    IOptions<ServiceSettings> options)
{
    private const string ClientName = "OpenWeather";
    private const string CurrentWeatherPath = "data/2.5/weather";

    private readonly ServiceSettings settings = options.Value;

    public async Task<Forecast?> GetCurrentWeatherByCityNameAsync(
        string city,
        CancellationToken cancellationToken)
    {
        var uri = BuildWeatherUri(city: city, cityId: null, mode: null);

        return await GetJsonAsync<Forecast>(uri, cancellationToken);
    }

    public async Task<Forecast?> GetCurrentWeatherByCityIdAsync(
        int cityId,
        CancellationToken cancellationToken)
    {
        var uri = BuildWeatherUri(city: null, cityId: cityId, mode: null);

        return await GetJsonAsync<Forecast>(uri, cancellationToken);
    }

    public async Task<Current?> GetCurrentWeatherInXmlByCityNameAsync(
        string city,
        CancellationToken cancellationToken)
    {
        var uri = BuildWeatherUri(city: city, cityId: null, mode: "xml");

        var xml = await GetStringAsync(uri, cancellationToken);

        return !string.IsNullOrWhiteSpace(xml)
            ? DeserializeXml<Current>(xml)
            : null;
    }

    private Uri BuildWeatherUri(string? city, int? cityId, string? mode)
    {
        if (!string.IsNullOrWhiteSpace(city) == (cityId is not null))
        {
            throw new ArgumentException("Provide either city or cityId, but not both.");
        }

        // Base required query parameters
        var query = new Dictionary<string, string?>
        {
            ["appid"] = settings.ApiKey,
            ["units"] = "metric",
        };

        // Choose exactly one identifier
        if (!string.IsNullOrWhiteSpace(city))
        {
            query["q"] = city;
        }

        if (cityId is not null)
        {
            query["id"] = cityId.Value.ToString();
        }

        if (!string.IsNullOrWhiteSpace(mode))
        {
            query["mode"] = mode;
        }

        var relative = QueryHelpers.AddQueryString(CurrentWeatherPath, query);
        return new Uri(relative, UriKind.Relative);
    }

    private async Task<T?> GetJsonAsync<T>(Uri relativeUri, CancellationToken ct)
    {
        var client = clientFactory.CreateClient(ClientName);
        var pipeline = pipelineProvider.GetPipeline(resiliencePipelineOptions.Value.Name);

        using var response = await pipeline.ExecuteAsync(
            async token => await client.GetAsync(relativeUri, token),
            ct);

        if (!response.IsSuccessStatusCode)
            return default;

        return await response.Content.ReadFromJsonAsync<T>(cancellationToken: ct);
    }

    private async Task<string?> GetStringAsync(Uri relativeUri, CancellationToken ct)
    {
        var client = clientFactory.CreateClient(ClientName);
        var pipeline = pipelineProvider.GetPipeline(resiliencePipelineOptions.Value.Name);

        using var response = await pipeline.ExecuteAsync(
            async token => await client.GetAsync(relativeUri, token),
            ct);

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadAsStringAsync(ct);
    }

    private static T DeserializeXml<T>(string xml)
    {
        var serializer = new XmlSerializer(typeof(T));
        using var reader = new StringReader(xml);
        return (T)serializer.Deserialize(reader)!;
    }
}

public class WeatherClient123(
    HttpClient httpClient,
    IOptions<ResiliencePipeline> resiliencePipelineOptions,
    IHttpClientFactory clientFactory,
    ResiliencePipelineProvider<string> pipelineProvider,
    IOptions<ServiceSettings> options)
{
    private readonly ServiceSettings serviceSettings = options.Value;

    public async Task<Current> GetCurrentWeatherInXmlByCityNameAsync(string city = "Tokio")
    {
        Current currentWeather = null;
        var url =
            $"https://{serviceSettings.OpenWeatherHost}/data/2.5/weather?q={city}&appid={serviceSettings.ApiKey}&mode=xml&units=metric";

        using HttpResponseMessage responseMessage = await httpClient.GetAsync(url);
        using HttpContent content = responseMessage.Content;
        string data = await content.ReadAsStringAsync();

        if (data != null)
        {
            XmlSerializer serializer = new(typeof(Current));
            using StringReader reader = new(data);

            currentWeather = (Current)serializer.Deserialize(reader);
        }

        return currentWeather;
    }

    public async Task<Forecast?> GetCurrentWeatherByCityNameAsync(
        string city,
        CancellationToken cancellationToken)
    {
        var requestUri =
            $"https://{serviceSettings.OpenWeatherHost}/data/2.5/weather?q={city}&appid={serviceSettings.ApiKey}&units=metric";

        var client = clientFactory.CreateClient();
        var pipeline = pipelineProvider.GetPipeline(resiliencePipelineOptions.Value.Name);
        HttpResponseMessage? response = null;
        Forecast? forecast = null;

        try
        {
            response = await pipeline.ExecuteAsync(
                async token => await client.GetAsync(requestUri, token),
                cancellationToken);

            forecast = await response.Content.ReadFromJsonAsync<Forecast>(cancellationToken);
        }
        finally
        {
            response?.Dispose();
        }

        return forecast;
    }

    public async Task<Forecast> GetCurrentWeatherByCityNameMockAsync(string city)
    {
        Coord coord = new(Convert.ToDecimal(16.9299), Convert.ToDecimal(52.4069));
        Weather weather = new(801, "Clouds", "broken clouds", "02d");
        Main main = new(Convert.ToDecimal(2.01), Convert.ToDecimal(-3.32), Convert.ToDecimal(1.11),
            Convert.ToDecimal(2.78), 1024, 51);
        int visibility = 10000;
        Wind wind = new(Convert.ToDecimal(3.6), 250);
        Clouds clouds = new(20);
        long dt = 1616253901;
        Sys sys = new(1, 1710, "PL", 1616216087, 1616259880);
        int timezone = 3600;
        int id = 3088171;
        string name = "Poznań";
        int cod = 200;

        var forecast = new Forecast(
            coord,
            [weather],
            main,
            visibility,
            wind,
            clouds,
            dt,
            sys,
            timezone,
            id,
            name,
            cod);

        return forecast;
    }

    public async Task<Forecast> GetCurrentWeatherByCityIdAsync(
        decimal cityId,
        CancellationToken cancellationToken)
    {
        var forecast = await httpClient.GetFromJsonAsync<Forecast>(
            $"https://{serviceSettings.OpenWeatherHost}/data/2.5/weather?id={cityId}&appid={serviceSettings.ApiKey}&units=metric",
            cancellationToken);

        return forecast;
    }

    private Uri BuildCurrentWeatherUri(string city)
    {
        var query = new Dictionary<string, string?>
        {
            ["q"] = city,
            ["appid"] = serviceSettings.ApiKey,
            ["units"] = "metric"
        };

        var uriString = QueryHelpers.AddQueryString("data/2.5/weather", query);
        return new Uri(uriString, UriKind.Relative);
    }
}