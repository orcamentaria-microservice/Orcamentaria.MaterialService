using Orcamentaria.Lib.Domain.Exceptions;
using Orcamentaria.MaterialService.Domain.Models;
using Orcamentaria.MaterialService.Domain.Repositories;
using Orcamentaria.MaterialService.Domain.Services;

namespace Orcamentaria.MaterialService.Application.Services
{
    public class MaterialSupplierService : IMaterialSupplierService
    {
        private readonly IMaterialSupplierRepository _repository;

        public MaterialSupplierService(IMaterialRepository repository)
        {
        }

        public async Task<MaterialSupplier?> GetByIdAsync(long id)
        {
            try
            {
                return await _repository.GetByIdAsync(id);
            }
            catch (DefaultException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new UnexpectedException(ex.Message, ex);
            }
        }
    }
}
