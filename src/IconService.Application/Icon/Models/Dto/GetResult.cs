namespace IconService.Application.Icon.Models.Dto;

public class GetResult
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Icon { get; set; } = null!;
    public bool DayIcon { get; set; }
    public byte[] FileContent { get; set; } = null!;
}