using System.Collections.Generic;
using CitiesGrpcService;

namespace GrpcCitiesClient
{
    public interface ICitiesClient
    {
        IAsyncEnumerable<City> GetCitiesStream(int pageNumber = 1, int numberOfCities = 25);
    }
}