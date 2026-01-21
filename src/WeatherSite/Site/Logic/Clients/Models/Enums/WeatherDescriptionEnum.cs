using System.ComponentModel;

namespace WeatherSite.Logic.Clients.Models.Enums;

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