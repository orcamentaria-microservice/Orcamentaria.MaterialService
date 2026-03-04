using Orcamentaria.Lib.Domain.Enums;
using Orcamentaria.Lib.Domain.Exceptions;
using Orcamentaria.Lib.Domain.Models;
using Orcamentaria.Lib.Domain.Models.Exceptions;
using Orcamentaria.MaterialService.Domain.Models;
using Orcamentaria.MaterialService.Domain.Repositories;
using Orcamentaria.MaterialService.Domain.Services;

namespace Orcamentaria.MaterialService.Application.Services
{
    public class MaterialSupplierService : IMaterialSupplierService
    {
        private readonly IMaterialSupplierRepository<MaterialSupplier> _repository;

        public MaterialSupplierService(IMaterialSupplierRepository<MaterialSupplier> repository)
        {
            _repository = repository;
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

        public async Task<IEnumerable<MaterialSupplier>?> GetAsync(GridParams gridParams)
        {
            try
            {
                var (data, _) = await _repository.GetAsync(gridParams);

                if (!data.Any())
                    throw new InfoException($"Nenhum dado foi encontrado.", ErrorCodeEnum.NotFound);

                return data;
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
