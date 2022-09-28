namespace CitiesService.Application.Models.Dto;

public class Coord
{
    public decimal Lon { get; set; }
    public decimal Lat { get; set; }
}

public class GetCityResult
{
    public decimal Id { get; set; }
    public string? Name { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public Coord? Coord { get; set; }
}