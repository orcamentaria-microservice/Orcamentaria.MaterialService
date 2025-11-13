using Orcamentaria.MaterialService.Domain.DTOs.MaterialType;
using Orcamentaria.MaterialService.Domain.DTOs.Person;

namespace Orcamentaria.MaterialService.Domain.DTOs.Material
{
    public class MaterialResponseDTO
    {
        public long Id { get; set; }
        public string? Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Manufacturer { get; set; }
        public MaterialTypeResponseDTO Type { get; set; }
        public bool Active { get; set; }
        public IEnumerable<PersonResponseDTO> Suppliers { get; set; } = new List<PersonResponseDTO>();
        public DateTime CreatedAt { get; set; }
        public long CreatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public long UpdatedBy { get; set; }
    }
}
