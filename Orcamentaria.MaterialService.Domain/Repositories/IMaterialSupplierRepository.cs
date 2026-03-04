using Orcamentaria.Lib.Domain.Models;
using Orcamentaria.Lib.Domain.Models.Responses;
using Orcamentaria.MaterialService.Domain.Models;
using System.Linq.Expressions;

namespace Orcamentaria.MaterialService.Domain.Repositories
{
    public interface IMaterialSupplierRepository<TEntity> where TEntity : class
    {
        Task<TEntity?> GetByIdAsync(long id, params Expression<Func<TEntity, object>>[] includes);
        Task<(IEnumerable<TEntity?>, ResponsePagination pagination)> GetAsync(GridParams gridParams, params Expression<Func<TEntity, object>>[] includes);
    }
}
