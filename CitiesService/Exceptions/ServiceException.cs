using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CitiesService.Exceptions
{
    public class ServiceException : Exception
    {
        public string Code { get; init; }

        public ServiceException(
            string code,
            string message,
            params object[] args) : base(string.Format(message, args), null)
        {
            Code = code;
        }
    }
}
