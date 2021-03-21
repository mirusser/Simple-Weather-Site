using AutoMapper;
using CitiesService.Data;
using CitiesService.Data.DatabaseModels;
using CitiesService.Logic.Helpers;
using CitiesService.Logic.Managers.Contracts;
using CitiesService.Logic.Repositories.Contracts;
using CitiesService.Models;
using CitiesService.Settings;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace CitiesService.Logic.Managers
{
    public class CityManager : ICityManager
    {
        private readonly FileUrlsAndPaths _fileUrlsAndPaths;
        private readonly IMapper _mapper;
        private readonly IGenericRepository<CityInfo> _cityInfoRepo;

        public CityManager(
            IOptions<FileUrlsAndPaths> options,
            IMapper mapper,
            IGenericRepository<CityInfo> cityInfoRepo)
        {
            _fileUrlsAndPaths = options.Value;
            _mapper = mapper;
            _cityInfoRepo = cityInfoRepo;
        }

        public async Task<bool> DownloadCityFile()
        {
            if (!File.Exists(_fileUrlsAndPaths.CompressedCityListFilePath))
            {
                using var client = new WebClient();
                client.DownloadFile(_fileUrlsAndPaths.CityListFileUrl, _fileUrlsAndPaths.CompressedCityListFilePath);
            }

            if (!File.Exists(_fileUrlsAndPaths.DecompressedCityListFilePath))
            {
                var fileInfo = new FileInfo(_fileUrlsAndPaths.DecompressedCityListFilePath);

                GzipHelper.Decompress(fileInfo);
            }

            return File.Exists(_fileUrlsAndPaths.DecompressedCityListFilePath);
        }

        public async Task<bool> SaveCitiesFromFileToDatabase()
        {
            var result = false;

            if (File.Exists(_fileUrlsAndPaths.DecompressedCityListFilePath))
            {
                List<City> citiesFromJson = new();

                using StreamReader streamReader = new(_fileUrlsAndPaths.DecompressedCityListFilePath);
                string json = streamReader.ReadToEnd();
                citiesFromJson = JsonConvert.DeserializeObject<List<City>>(json);

                var cityInfo = _mapper.Map<CityInfo>(citiesFromJson.First());
                var cityInfos = _mapper.Map<List<CityInfo>>(citiesFromJson);

                result = await _cityInfoRepo.CreateRange(cityInfos);
                result = result && await _cityInfoRepo.Save();
            }

            return result;
        }
    }
}
