using Microsoft.EntityFrameworkCore;
using Orcamentaria.Lib.Domain.Contexts;
using Orcamentaria.Lib.Domain.Exceptions;
using Orcamentaria.Lib.Infrastructure.Repositories;
using Orcamentaria.MaterialService.Domain.Models;
using Orcamentaria.MaterialService.Domain.Repositories;
using Orcamentaria.MaterialService.Infrastructure.Contexts;

namespace Orcamentaria.MaterialService.Infrastructure.Repositories
{
    public class MaterialRepository : BaseRepository<Material>, IMaterialRepository<Material>
    {
        private readonly MySqlContext _context;
        private readonly IUserAuthContext _userAuthContext;

        public MaterialRepository(
            MySqlContext context, 
            IUserAuthContext userAuthContext) 
            : base(context, userAuthContext)
        {
            _context = context;
            _userAuthContext = userAuthContext;
        }

        public async Task<Material> AddSuppliersAsync(long materialId, IEnumerable<MaterialSupplier> suppliers)
        {
            try
            {
                var materialEntity = _context.Materials
                    .Include(x => x.Suppliers)
                    .First(x => x.Id == materialId && x.CompanyId == _userAuthContext.CompanyId);

                materialEntity.Suppliers = materialEntity.Suppliers.Union(suppliers).ToList();

                await _context.SaveChangesAsync();

                return materialEntity;
            }
            catch (Exception ex)
            {
                throw new DatabaseException(ex.Message, ex);
            }
        }

        public async Task<Material> RemoveSuppliersAsync(long materialId, IEnumerable<MaterialSupplier> suppliers)
        {
            try
            {
                var userEntity = _context.Materials
                    .Include(x => x.Suppliers)
                    .First(x => x.Id == materialId && x.CompanyId == _userAuthContext.CompanyId);

                foreach (var supplier in suppliers)
                {
                    userEntity.Suppliers.Remove(supplier);
                }

                await _context.SaveChangesAsync();

                return userEntity;
            }
            catch (Exception ex)
            {
                throw new DatabaseException(ex.Message, ex);
            }
        }
    }
}
