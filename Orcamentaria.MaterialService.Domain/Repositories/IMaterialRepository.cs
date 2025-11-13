using Orcamentaria.Lib.Domain.Repositories;
using Orcamentaria.MaterialService.Domain.Models;
using System.Security;

namespace Orcamentaria.MaterialService.Domain.Repositories
{
    public interface IMaterialRepository : IBasicRepository<Material>
    {
        Task<Material> AddSuppliersAsync(long materialId, IEnumerable<MaterialSupplier> suppliers);
        Task<Material> RemoveSuppliersAsync(long materialId, IEnumerable<MaterialSupplier> suppliers);
    }
}
