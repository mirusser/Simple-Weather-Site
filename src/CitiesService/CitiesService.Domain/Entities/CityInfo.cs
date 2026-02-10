using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CitiesService.Domain.Entities;

public class CityInfo
{
    [Key] public int Id { get; init; }

    /// <summary>
    /// This is a city `id` from JSON file
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal CityId { get; init; }

    [Required] public string Name { get; init; } = null!;

    public string? State { get; init; }

    [Required] public string CountryCode { get; init; } = null!;

    [Column(TypeName = "decimal(9,6)")] 
    public decimal Lon { get; init; }

    [Column(TypeName = "decimal(9, 6)")] 
    public decimal Lat { get; init; }
}