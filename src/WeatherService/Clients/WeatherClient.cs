using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Common.ExtensionMethods;
using Common.Infrastructure.Managers.Contracts;
using Common.Presentation.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
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

    // TODO: use it in tests
    public Task<Forecast> GetCurrentWeatherByCityNameMockAsync(string city)
    {
        Coord coord = new(Convert.ToDecimal(16.9299), Convert.ToDecimal(52.4069));
        Weather weather = new(801, "Clouds", "broken clouds", "02d");
        Main main = new(
            Convert.ToDecimal(2.01),
            Convert.ToDecimal(-3.32),
            Convert.ToDecimal(1.11),
            Convert.ToDecimal(2.78),
            1024, 51);
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

        return Task.FromResult(forecast);
    }
}