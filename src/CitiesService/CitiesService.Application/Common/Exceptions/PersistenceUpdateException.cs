using System;

namespace CitiesService.Application.Common.Exceptions;

public sealed class PersistenceUpdateException(string message, Exception? innerException = null)
    : Exception(message, innerException);
