using Orcamentaria.Lib.Domain.Contexts;
using Orcamentaria.Lib.Domain.Exceptions;
using Orcamentaria.Lib.Infrastructure.Repositories;
using Orcamentaria.MaterialService.Domain.Models;
using Orcamentaria.MaterialService.Domain.Repositories;
using Orcamentaria.MaterialService.Infrastructure.Contexts;

namespace Orcamentaria.MaterialService.Infrastructure.Repositories
{
    public class MaterialTypeRespository : BasicRepository<MaterialType>, IMaterialTypeRepository
    {
        private readonly MySqlContext _context;

        public MaterialTypeRespository(
            MySqlContext context, 
            IUserAuthContext userAuthContext) 
            : base(context, userAuthContext)
        {
            _context = context;
        }

        public int CountItems(long materialId)
        {
            try
            {
                return _context.MaterialSuppliers.Where(x => x.MaterialId == materialId).Count();
            }
            catch (Exception ex)
            {
                throw new DatabaseException(ex.Message, ex);
            }
        }
    }
}
