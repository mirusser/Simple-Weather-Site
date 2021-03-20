using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace WeatherSite.Clients.Models.Enums
{
    public enum WeatherDescriptionEnum
    {
        [Description("clear sky")]
        ClearSky,

        [Description("few clouds")]
        FewClouds,

        [Description("scattered clouds")]
        ScatteredClouds,

        [Description("broken clouds")]
        BrokenClouds,

        [Description("shower rain")]
        ShowerRain,

        [Description("rain")]
        Rain,

        [Description("thunderstorm")]
        Thunderstorm,

        [Description("snow")]
        Snow,

        [Description("mist")]
        Mist
    }
}
