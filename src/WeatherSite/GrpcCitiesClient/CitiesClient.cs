using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.ClientFactory;
using CitiesGrpcService;
using Grpc.Net.Client.Configuration;

namespace GrpcCitiesClient
{
    public class CitiesClient : ICitiesClient
    {
        private readonly Cities.CitiesClient _client;

        public CitiesClient(GrpcClientFactory grpcClientFactory)
        {
            var defaultMethodConfig = new MethodConfig
            {
                Names = { MethodName.Default },
                RetryPolicy = new RetryPolicy
                {
                    MaxAttempts = 5,
                    InitialBackoff = TimeSpan.FromSeconds(1),
                    MaxBackoff = TimeSpan.FromSeconds(5),
                    BackoffMultiplier = 1.5,
                    RetryableStatusCodes = { StatusCode.Unavailable }
                }
            };

            _client = grpcClientFactory.CreateClient<Cities.CitiesClient>("Cities");
        }

        public async IAsyncEnumerable<CityReply> GetCitiesStream(int pageNumber = 1, int numberOfCities = 25)
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

        public async Task<CitiesPaginationReply> GetCitiesPagination(int pageNumber = 1, int numberOfCities = 25)
        {
            var request = new CitiesPaginationRequest()
            {
                PageNumber = pageNumber,
                NumberOfCities = numberOfCities
            };

           var response = await _client.GetCitiesPaginationAsync(request);

            return response;
        }
    }
}
