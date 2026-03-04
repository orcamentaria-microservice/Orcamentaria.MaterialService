using Orcamentaria.Lib.Domain.Models;
using Orcamentaria.Lib.Domain.Models.Responses;
using Orcamentaria.MaterialService.Domain.Models;
using System.Linq.Expressions;

namespace Orcamentaria.MaterialService.Domain.Repositories
{
    public interface IMaterialRepository<TEntity> where TEntity : class
    {
        Task<TEntity?> GetByIdAsync(long id, params Expression<Func<TEntity, object>>[] includes);
        Task<(IEnumerable<TEntity?>, ResponsePagination pagination)> GetAsync(GridParams gridParams, params Expression<Func<TEntity, object>>[] includes);
        Task<TEntity> InsertAsync(TEntity entity);
        Task<TEntity> UpdateAsync(long id, TEntity entity);
        Task<Material> AddSuppliersAsync(long materialId, IEnumerable<MaterialSupplier> suppliers);
        Task<Material> RemoveSuppliersAsync(long materialId, IEnumerable<MaterialSupplier> suppliers);
    }
}
