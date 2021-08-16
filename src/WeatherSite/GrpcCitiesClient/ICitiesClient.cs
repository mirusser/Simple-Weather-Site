using System.Collections.Generic;
using CitiesGrpcService;

namespace GrpcCitiesClient
{
    public interface ICitiesClient
    {
        IAsyncEnumerable<CityReply> GetCitiesStream(int pageNumber = 1, int numberOfCities = 25);
    }
}