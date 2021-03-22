using CitiesService.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CitiesService.Logic.Managers.Contracts
{
    public interface ICityManager
    {
        Task<bool> DownloadCityFile();
        Task<bool> SaveCitiesFromFileToDatabase();
        Task<List<CityDto>> GetCitiesByName(string cityName, int limit = 10);
    }
}
