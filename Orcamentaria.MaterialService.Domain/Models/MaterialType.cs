using Orcamentaria.Lib.Domain.Entities;

namespace Orcamentaria.MaterialService.Domain.Models
{
    public class MaterialType : TenantEntity
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; } = true;
    }
}
