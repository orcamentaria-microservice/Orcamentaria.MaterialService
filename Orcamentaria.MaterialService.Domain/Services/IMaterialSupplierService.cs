using Orcamentaria.MaterialService.Domain.Models;

namespace Orcamentaria.MaterialService.Domain.Services
{
    public interface IMaterialSupplierService 
    {
        Task<MaterialSupplier?> GetByIdAsync(long id);
    }
}
