namespace Orcamentaria.MaterialService.Domain.Models
{
    public class MaterialSupplier
    {
        public long Id { get; set; }
        public long SupplierId { get; set; }
        public long MaterialId { get; set; }
        public long CompanyId { get; set; }
    }
}
