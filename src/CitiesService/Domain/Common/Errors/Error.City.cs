using ErrorOr;

namespace CitiesService.Domain.Common.Errors;

public static partial class Errors
{
    public static class City
    {
        //public static Error IconNotCreated => Error.Failure(
        //    code: "Icon.NotCreated",
        //    description: "Icon wasn't created");

        public static Error CityNotFound => Error.NotFound(
            code: "City.NotFound",
            description: "City not found");
    }
}