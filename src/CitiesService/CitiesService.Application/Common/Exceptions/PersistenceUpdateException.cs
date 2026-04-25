using System;

namespace CitiesService.Application.Common.Exceptions;

public sealed class PersistenceUpdateException : Exception
{
    public PersistenceUpdateException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
