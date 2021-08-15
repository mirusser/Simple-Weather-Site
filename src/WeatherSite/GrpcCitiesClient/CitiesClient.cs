using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.ClientFactory;
using CitiesGrpcService;

namespace GrpcCitiesClient
{
    public class CitiesClient : ICitiesClient
    {
        private readonly Cities.CitiesClient _client;

        public CitiesClient(GrpcClientFactory grpcClientFactory)
        {
            _client = grpcClientFactory.CreateClient<Cities.CitiesClient>("Cities");
        }

        public async IAsyncEnumerable<City> GetCitiesStream(int pageNumber = 1, int numberOfCities = 25)
        {
            var request = new CitiesStreamRequest()
            {
                PageNumber = pageNumber,
                NumberOfCities = numberOfCities
            };

            using var call = _client.GetCitiesStream(request);

            await foreach (var cityReply in call.ResponseStream.ReadAllAsync())
            {
                yield return cityReply;
            }
        }
    }
}
