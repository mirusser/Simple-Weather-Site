namespace CitiesService.Application.Telemetry;

public static class CitiesTelemetryConventions
{
    public static class ActivitySources
    {
        public const string Application = "CitiesService.Application";
        public const string GraphQl = "CitiesService.GraphQL";
        public const string Grpc = "CitiesGrpcService";
    }

    public static class Meters
    {
        public const string Application = "CitiesService.Application";
        public const string Grpc = "CitiesGrpcService";
    }

    public static class MetricNames
    {
        public const string MediatorRequestDuration = "sws.cities.mediator.request.duration";
        public const string MediatorRequests = "sws.cities.mediator.requests";
        public const string CacheRequests = "sws.cities.cache.requests";
        public const string ReturnedCities = "sws.cities.returned";
        public const string SeedingRuns = "sws.cities.seeding.runs";
        public const string SeedingDuration = "sws.cities.seeding.duration";
        public const string GrpcCalls = "sws.cities.grpc.calls";
        public const string GrpcCallDuration = "sws.cities.grpc.call.duration";
        public const string GrpcStreamMessages = "sws.cities.grpc.stream.messages";
    }

    public static class MetricUnits
    {
        public const string Seconds = "s";
        public const string Request = "{request}";
        public const string City = "{city}";
        public const string Run = "{run}";
        public const string Call = "{call}";
        public const string Message = "{message}";
    }

    public static class MetricDescriptions
    {
        public const string MediatorRequestDuration = "Duration of CitiesService mediator requests.";
        public const string MediatorRequests = "Number of CitiesService mediator requests.";
        public const string CacheRequests = "Number of CitiesService cache lookups.";
        public const string ReturnedCities = "Number of cities returned by CitiesService operations.";
        public const string SeedingRuns = "Number of CitiesService seeding attempts.";
        public const string SeedingDuration = "Duration of CitiesService seeding attempts.";
        public const string GrpcCalls = "Number of Cities gRPC calls.";
        public const string GrpcCallDuration = "Duration of Cities gRPC calls.";
        public const string GrpcStreamMessages = "Number of Cities gRPC stream messages sent.";
    }

    public static class TagNames
    {
        public const string Operation = "operation";
        public const string Result = "result";
        public const string ErrorType = "error_type";
        public const string CacheResult = "cache_result";
        public const string GrpcMethod = "grpc_method";
        public const string GrpcType = "grpc_type";
        public const string HttpStatusCode = "http.status_code";
    }

    public static class ResultValues
    {
        public const string Success = "success";
        public const string Failure = "failure";
        public const string Failed = "failed";
        public const string Exception = "exception";
        public const string NotFound = "not_found";
        public const string Deferred = "deferred";
    }

    public static class CacheResults
    {
        public const string Hit = "hit";
        public const string Miss = "miss";
    }

    public static class SeedingResults
    {
        public const string AlreadyExists = "already_exists";
        public const string LockNotAcquired = "lock_not_acquired";
        public const string Seeded = "seeded";
        public const string Failed = ResultValues.Failed;
        public const string DownloadFailed = "download_failed";
        public const string ConcurrencyAccepted = "concurrency_accepted";
    }

    public static class Operations
    {
        public const string GetCitiesByName = "GetCitiesByName";
        public const string GetCitiesPagination = "GetCitiesPagination";
        public const string CitiesSeedIfEmpty = "CitiesSeedIfEmpty";
        public const string CitiesSeedDatabaseWrite = "CitiesSeedDatabaseWrite";
        public const string CitiesDownloadCityList = "CitiesDownloadCityList";

        public static class GraphQl
        {
            public const string GetCities = "GraphQL.GetCities";
            public const string GetCityByDbId = "GraphQL.GetCityByDbId";
            public const string GetCityByCityId = "GraphQL.GetCityByCityId";
            public const string UpdateCity = "GraphQL.UpdateCity";
            public const string PatchCity = "GraphQL.PatchCity";
        }

        public static class Grpc
        {
            public const string GetCitiesPaginationInfo = "cities.Cities/GetCitiesPaginationInfo";
            public const string GetCitiesPagination = "cities.Cities/GetCitiesPagination";
            public const string GetCitiesStream = "cities.Cities/GetCitiesStream";
        }
    }

    public static class GrpcTypes
    {
        public const string Unary = "unary";
        public const string ServerStreaming = "server_streaming";
    }
}
