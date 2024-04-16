using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CitiesService.Application.Features.City.Models.Dto;
using CitiesService.Application.Common.Helpers;
using CitiesService.Application.Common.Interfaces.Persistence;
using CitiesService.Domain.Entities;
using CitiesService.Domain.Settings;
using ErrorOr;
using MapsterMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CitiesService.Application.Features.City.Commands.AddCitiesToDatabase;

public class AddCitiesToDatabaseCommand : IRequest<ErrorOr<AddCitiesToDatabaseResult>>;

// TODO: better httpClient (HttpClientFactory maybe?)
public class AddCitiesToDatabaseHandler(
	IGenericRepository<CityInfo> cityInfoRepo,
	IOptions<FileUrlsAndPaths> options,
	IMapper mapper,
	HttpClient httpClient,
	ILogger<AddCitiesToDatabaseHandler> logger,
	JsonSerializerOptions jsonSerializerOptions) : IRequestHandler<AddCitiesToDatabaseCommand, ErrorOr<AddCitiesToDatabaseResult>>
{
	private readonly FileUrlsAndPaths fileUrlsAndPaths = options.Value;
	private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

	public async Task<ErrorOr<AddCitiesToDatabaseResult>> Handle(
		AddCitiesToDatabaseCommand request,
		CancellationToken cancellationToken)
	{
		var anyCityExists = await cityInfoRepo
			.CheckIfExistsAsync(c => c.Id != default, cancellationToken);

		if (anyCityExists)
		{
			return new AddCitiesToDatabaseResult { IsSuccess = false, IsAlreadyAdded = anyCityExists };
		}

		var isSuccess = await SaveCitiesFromFileToDatabase(cancellationToken);

		return new AddCitiesToDatabaseResult { IsSuccess = isSuccess, IsAlreadyAdded = anyCityExists };
	}

	private async Task<bool> SaveCitiesFromFileToDatabase(CancellationToken cancellationToken)
	{
		var downloadResult = await DownloadCityFileAsync(cancellationToken);
		if (!downloadResult)
		{
			return false;
		}

		using StreamReader streamReader = new(fileUrlsAndPaths.DecompressedCityListFilePath);
		string? json = streamReader.ReadToEnd();

		List<GetCityResult>? citiesFromJson = JsonSerializer.Deserialize<List<GetCityResult>>(
			json,
			jsonSerializerOptions);
		citiesFromJson ??= [];

		var cityInfo = mapper.Map<List<CityInfo>>(citiesFromJson);

		await cityInfoRepo.CreateRangeAsync(cityInfo, cancellationToken);
		return await cityInfoRepo.SaveAsync(cancellationToken);
	}

	private async Task<bool> DownloadCityFileAsync(CancellationToken cancellationToken)
	{
		if (!File.Exists(fileUrlsAndPaths.CompressedCityListFilePath))
		{
			await DownloadFileAsync(
				fileUrlsAndPaths.CityListFileUrl,
				fileUrlsAndPaths.CompressedCityListFilePath,
				cancellationToken);
		}

		if (!File.Exists(fileUrlsAndPaths.DecompressedCityListFilePath))
		{
			var fileInfo = new FileInfo(fileUrlsAndPaths.CompressedCityListFilePath);
			await GzipHelper.DecompressAsync(fileInfo, logger, cancellationToken);
		}

		return File.Exists(fileUrlsAndPaths.DecompressedCityListFilePath);
	}

	private async Task DownloadFileAsync(
		string requestUri,
		string filename,
		CancellationToken cancellationToken)
	{
		using var response = await _httpClient.GetAsync(
			requestUri,
			HttpCompletionOption.ResponseHeadersRead,
			cancellationToken);
		response.EnsureSuccessStatusCode();

		await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
		await using FileStream fileStream = new(filename, FileMode.Create, FileAccess.Write, FileShare.None);
		await stream.CopyToAsync(fileStream, cancellationToken);
	}
}