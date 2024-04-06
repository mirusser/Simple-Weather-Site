using ErrorOr;

namespace IconService.Domain.Common.Errors;

public static partial class Errors
{
    public static class Icon
    {
        public static Error IconNotCreated => Error.Failure(
            code: "Icon.NotCreated",
            description: "Icon wasn't created");

        public static Error IconNotFound => Error.NotFound(
            code: "Icon.NotFound",
            description: "Icon not found");
    }
}