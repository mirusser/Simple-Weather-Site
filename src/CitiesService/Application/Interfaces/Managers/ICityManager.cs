using Application.Dto;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Interfaces.Managers
{
    public interface ICityManager
    {
        Task<List<CityDto>> GetCitiesByName(string cityName, int limit = 10);
        Task<CitiesPaginationDto> GetCitiesPaginationDto(int numberOfCities = 25, int pageNumber = 1);
        Task<CityInfoPaginationDto> GetCitiesInfoPagination(int numberOfCities = 25, int pageNumber = 1);
        Task<int> GetCountOfAllCities();
        bool DownloadCityFile();
        Task<bool> SaveCitiesFromFileToDatabase();
    }
}
