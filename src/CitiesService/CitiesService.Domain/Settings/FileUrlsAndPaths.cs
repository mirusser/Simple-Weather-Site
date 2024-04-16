namespace CitiesService.Domain.Settings;

public class FileUrlsAndPaths
{
    public string CityListFileUrl { get; set; } = null!;
    public string CompressedCityListFilePath { get; set; } = null!;
    public string DecompressedCityListFilePath { get; set; } = null!;
}