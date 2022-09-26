using System.Collections.Generic;
using System.Threading.Tasks;
using CitiesService.Application.Dto;

namespace CitiesService.Application.Interfaces.Managers;

public interface ICityManager
{
    Task<List<CityDto>> GetCitiesByName(string cityName, int limit = 10);

    Task<CitiesPaginationDto> GetCitiesPaginationDto(int numberOfCities = 25, int pageNumber = 1);

    Task<CityInfoPaginationDto> GetCitiesInfoPagination(int numberOfCities = 25, int pageNumber = 1);

    Task<int> GetCountOfAllCities();

    bool DownloadCityFile();

    Task<bool> SaveCitiesFromFileToDatabase();
}