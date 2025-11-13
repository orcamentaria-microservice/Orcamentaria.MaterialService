using Orcamentaria.Lib.Domain.Entities;

namespace Orcamentaria.MaterialService.Domain.Models
{
    public class Material : TenantEntity
    {
        public long Id { get; set; }
        public string? Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Manufacturer { get; set; }
        public long TypeId { get; set; }
        public MaterialType Type { get; set; }
        public bool Active { get; set; }
        public ICollection<MaterialSupplier> Suppliers { get; set; }
    }
}
