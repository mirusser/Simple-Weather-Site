using System;

namespace CitiesService.Application.Common.Exceptions;

public sealed class PersistenceConcurrencyException : Exception
{
    public PersistenceConcurrencyException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
