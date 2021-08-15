using Application.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Interfaces.Managers
{
    public interface ICityManager
    {
        Task<List<CityDto>> GetCitiesByName(string cityName, int limit = 10);
        Task<CitiesPaginationDto> GetCitiesPagination(int numberOfCities = 25, int pageNumber = 1);
        bool DownloadCityFile();
        Task<bool> SaveCitiesFromFileToDatabase();
    }
}
