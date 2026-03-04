using Orcamentaria.Lib.Domain.Models;
using Orcamentaria.MaterialService.Domain.Models;

namespace Orcamentaria.MaterialService.Domain.Services
{
    public interface IMaterialSupplierService 
    {
        Task<IEnumerable<MaterialSupplier>?> GetAsync(GridParams gridParams);
        Task<MaterialSupplier?> GetByIdAsync(long id);
    }
}
