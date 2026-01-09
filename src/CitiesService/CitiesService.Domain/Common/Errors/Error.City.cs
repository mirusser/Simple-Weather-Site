using Common.Domain.Errors;

namespace CitiesService.Domain.Common.Errors;

public readonly record struct Errors
{
    public static class City
    {
        //public static Error IconNotCreated => Error.Failure(
        //    code: "Icon.NotCreated",
        //    description: "Icon wasn't created");

        public static Error CityNotFound => new(
            "City.NotFound",
            "City not found",
            ErrorType.NotFound);
    }
}