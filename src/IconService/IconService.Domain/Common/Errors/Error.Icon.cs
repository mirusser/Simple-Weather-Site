using Common.Presentation.Exceptions;

namespace IconService.Domain.Common.Errors;

public static partial class Errors
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