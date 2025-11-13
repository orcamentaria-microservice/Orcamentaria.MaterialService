using Orcamentaria.Lib.Domain.Repositories;
using Orcamentaria.MaterialService.Domain.Models;

namespace Orcamentaria.MaterialService.Domain.Repositories
{
    public interface IMaterialTypeRepository : IBasicRepository<MaterialType>
    {
        int CountItems(long materialId);
    }
}
