using System.Collections.Generic;
using System.Threading.Tasks;
using CitiesGrpcService;

namespace GrpcCitiesClient
{
    public interface ICitiesClient
    {
        IAsyncEnumerable<CityReply> GetCitiesStream(int pageNumber = 1, int numberOfCities = 25);
        Task<CitiesPaginationReply> GetCitiesPagination(int pageNumber = 1, int numberOfCities = 25);
    }
}