namespace IconService.Models.Dto
{
    public class IconDto
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Icon { get; set; } = null!;
        public bool DayIcon { get; set; }
        public byte[] FileContent { get; set; } = null!;
    }
}