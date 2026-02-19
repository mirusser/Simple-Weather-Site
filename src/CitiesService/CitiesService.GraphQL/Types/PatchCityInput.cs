namespace CitiesService.GraphQL.Types;

public sealed class PatchCityInput
{
    public int Id { get; set; }

    public Optional<decimal?> CityId { get; set; }
    public Optional<string> Name { get; set; }
    public Optional<string?> State { get; set; }
    public Optional<string> CountryCode { get; set; }
    public Optional<decimal?> Lon { get; set; }
    public Optional<decimal?> Lat { get; set; }
    public Optional<string> RowVersion { get; set; }
}