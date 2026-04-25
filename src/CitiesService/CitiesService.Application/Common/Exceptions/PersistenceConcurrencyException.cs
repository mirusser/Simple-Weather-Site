using System;

namespace CitiesService.Application.Common.Exceptions;

public sealed class PersistenceConcurrencyException(string message, Exception? innerException = null)
    : Exception(message, innerException);
