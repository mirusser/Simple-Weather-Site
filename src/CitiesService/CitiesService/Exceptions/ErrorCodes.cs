using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CitiesService.Exceptions
{
    public static class ErrorCodes
    {
        public const string SqlException = "Database is not accessible";
        public const string ValidationException = "Validation exception";
    }
}
