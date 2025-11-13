namespace Orcamentaria.MaterialService.Domain.DTOs.Material
{
    public class MaterialUpdateDTO
    {
        public string? Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Manufacturer { get; set; }
        public long TypeId { get; set; }
        public bool Active { get; set; }
    }
}
