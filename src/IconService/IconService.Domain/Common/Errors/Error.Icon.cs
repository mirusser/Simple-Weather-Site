using Common.Domain.Errors;

namespace IconService.Domain.Common.Errors;

public readonly record struct Errors
{
    public static class Icon
    {
        public static Error IconNotCreated => new(
            "Icon.NotCreated",
            "Icon wasn't created",
            ErrorType.Failure);

        public static Error IconNotFound => new(
            "Icon.NotFound",
            "Icon not found",
            ErrorType.NotFound);
    }
}