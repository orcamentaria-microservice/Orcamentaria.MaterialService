using Orcamentaria.Lib.Domain.Contexts;
using Orcamentaria.Lib.Infrastructure.Repositories;
using Orcamentaria.MaterialService.Domain.Models;
using Orcamentaria.MaterialService.Domain.Repositories;
using Orcamentaria.MaterialService.Infrastructure.Contexts;

namespace Orcamentaria.MaterialService.Infrastructure.Repositories
{
    public class MaterialTypeRepository : BaseRepository<MaterialType>, IMaterialTypeRepository<MaterialType>
    {
        public MaterialTypeRepository(
            MySqlContext context, 
            IUserAuthContext userAuthContext) 
            : base(context, userAuthContext)
        {
        }
    }
}
