using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace WeatherSite.Logic.Helpers
{
    public static class UriHelper
    {
        public static string CreateAbsoluteUrl(string? url, Dictionary<string, string>? parameters, string? fragment = null)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException(nameof(url));
            }

            var uriBuilder = new UriBuilder(url);
            var uriParameters = HttpUtility.ParseQueryString(string.Empty);

            if (parameters is not null && parameters.Any())
            {
                foreach (var parameter in parameters)
                {
                    uriParameters[parameter.Key] = HttpUtility.UrlEncode(parameter.Value);
                }
            }

            uriBuilder.Query = uriParameters.ToString();

            if (!string.IsNullOrEmpty(fragment))
            {
                uriBuilder.Fragment = fragment;
            }

            Uri uri = uriBuilder.Uri;

            if (!Uri.TryCreate(uri.ToString(), UriKind.Absolute, out uri))
            {
                throw new ArgumentException(nameof(url));
            }

            return uri.OriginalString;
        }
    }
}
