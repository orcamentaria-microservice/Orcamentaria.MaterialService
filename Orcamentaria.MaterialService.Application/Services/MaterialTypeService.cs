using AutoMapper;
using Orcamentaria.MaterialService.Domain.Models;
using Orcamentaria.MaterialService.Domain.Repositories;
using Orcamentaria.MaterialService.Domain.Services;
using Orcamentaria.Lib.Domain.Enums;
using Orcamentaria.Lib.Domain.Exceptions;
using Orcamentaria.Lib.Domain.Models;
using Orcamentaria.Lib.Domain.Models.Exceptions;
using Orcamentaria.Lib.Domain.Models.Responses;
using Orcamentaria.Lib.Domain.Validators;
using Orcamentaria.MaterialService.Domain.DTOs.MaterialType;

namespace Orcamentaria.MaterialService.Application.Services
{
    public class MaterialTypeService : IMaterialTypeService
    {
        private readonly IMaterialTypeRepository<MaterialType> _repository;
        private readonly IValidatorEntity<MaterialType> _validator;
        private readonly IMapper _mapper;

        public MaterialTypeService(
            IMaterialTypeRepository<MaterialType> repository,
            IValidatorEntity<MaterialType> validator,
            IMapper mapper)
        {
            _repository = repository;
            _validator = validator;
            _mapper = mapper;
        }

        public async Task<MaterialType?> GetByIdAsync(long id)
        {
            try
            {
                return await _repository.GetByIdAsync(id);
            }
            catch (DefaultException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Response<IEnumerable<MaterialTypeResponseDTO>>?> GetAsync(GridParams gridParams)
        {
            try
            {
                var (data, pagination) = await _repository.GetAsync(gridParams);

                if (!data.Any())
                    throw new InfoException($"Nenhum dado foi encontrado.", ErrorCodeEnum.NotFound);

                return new Response<IEnumerable<MaterialTypeResponseDTO>>(
                        data.Select(x => _mapper.Map<MaterialType, MaterialTypeResponseDTO>(x)), pagination);
            }
            catch (DefaultException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Response<MaterialTypeResponseDTO>> InsertAsync(MaterialTypeInsertDTO dto)
        {
            try
            {
                var materialType = _mapper.Map<MaterialTypeInsertDTO, MaterialType>(dto);

                materialType.Name = materialType.Name.ToUpper();

                var result = _validator.ValidateBeforeInsert(materialType);

                if (!result.IsValid)
                    throw new ValidationException(result);

                var entity = await _repository.InsertAsync(materialType);

                return new Response<MaterialTypeResponseDTO>(
                    _mapper.Map<MaterialType, MaterialTypeResponseDTO>(entity));
            }
            catch (DefaultException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Response<MaterialTypeResponseDTO>> UpdateAsync(long id, MaterialTypeUpdateDTO dto)
        {
            try
            {
                var materialType = _mapper.Map<MaterialTypeUpdateDTO, MaterialType>(dto);

                materialType.Id = id;
                materialType.Name = materialType.Name.ToUpper();

                var result = _validator.ValidateBeforeUpdate(materialType);

                if (!result.IsValid)
                    throw new ValidationException(result);

                var entity = await _repository.UpdateAsync(id, materialType);

                return new Response<MaterialTypeResponseDTO>(
                    _mapper.Map<MaterialType, MaterialTypeResponseDTO>(entity));
            }
            catch (DefaultException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
