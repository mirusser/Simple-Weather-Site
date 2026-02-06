using System;
using System.Linq;

namespace WeatherSite.Logic.ExtensionMethods;

public static class StringExtensions
{
    extension(string input)
    {
        public string FirstCharToUpper() =>
            input switch
            {
                null => throw new ArgumentNullException(nameof(input)),
                "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
                _ => string.Concat(input.First().ToString().ToUpper(), input.AsSpan(1))
            };
    }
}