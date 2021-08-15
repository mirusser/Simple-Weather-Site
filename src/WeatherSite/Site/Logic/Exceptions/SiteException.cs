using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WeatherSite.Exceptions
{
    public class SiteException : Exception
    {
        public string Code { get; set; }

        public SiteException(
            string code,
            string message,
            params object[] args) : base(string.Format(message, args), null)
        {
            Code = code;
        }
    }
}
