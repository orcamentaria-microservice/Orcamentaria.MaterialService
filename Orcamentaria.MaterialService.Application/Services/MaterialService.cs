using AutoMapper;
using Orcamentaria.Lib.Domain.Enums;
using Orcamentaria.Lib.Domain.Exceptions;
using Orcamentaria.Lib.Domain.Models;
using Orcamentaria.Lib.Domain.Models.Exceptions;
using Orcamentaria.Lib.Domain.Models.Responses;
using Orcamentaria.MaterialService.Domain.DTOs.Material;
using Orcamentaria.MaterialService.Domain.Models;
using Orcamentaria.MaterialService.Domain.Repositories;
using Orcamentaria.MaterialService.Domain.ServiceClient;
using Orcamentaria.MaterialService.Domain.Services;
using Orcamentaria.MaterialService.Domain.Validators;

namespace Orcamentaria.MaterialService.Application.Services
{
    public class MaterialService : IMaterialService
    {
        private readonly IMaterialRepository _repository;
        private readonly IMaterialSupplierService _materialSupplierService;
        private readonly IPersonServiceClient _personServiceClient;
        private readonly IMaterialValidator _validator;
        private readonly IMapper _mapper; 

        public MaterialService(
            IMaterialRepository repository,
            IMaterialSupplierService materialSupplierService,
            IPersonServiceClient personServiceClient,
            IMaterialValidator validator,
            IMapper mapper)
        {
            _repository = repository;
            _materialSupplierService = materialSupplierService;
            _personServiceClient = personServiceClient;
            _validator = validator;
            _mapper = mapper;
        }

        public async Task<Material?> GetByIdAsync(long id)
        {
            try
            {
                return await _repository.GetByIdAsync(id, p => p.Type);
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

        public async Task<Response<IEnumerable<MaterialResponseDTO>>?> GetAsync(GridParams gridParams)
        {
            try
            {
                var (data, pagination) = await _repository.GetAsync(gridParams, p => p.Type, p => p.Suppliers);

                if (!data.Any())
                    throw new InfoException($"Nenhum dado foi encontrado.", ErrorCodeEnum.NotFound);

                var suppliersId = data
                    .SelectMany(item => item.Suppliers)
                    .Select(s => s.SupplierId)
                    .ToHashSet();

                var responseSuppliers = await _personServiceClient.GetSuppliersAsync(suppliersId);

                var responseData = data
                    .Select(x =>_mapper.Map<Material, MaterialResponseDTO>(x))
                    .ToList();

                foreach (var item in responseData)
                {
                    var supplierIds = data
                        .First(x => x!.Id == item.Id)!.Suppliers
                        .Select(x => x.SupplierId);

                    item.Suppliers = responseSuppliers.Data!
                                        .Where(s => supplierIds
                                        .Contains(s.Id))
                                        .ToList();
                }

                return new Response<IEnumerable<MaterialResponseDTO>>(responseData, pagination);
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

        public async Task<Response<MaterialResponseDTO>> InsertAsync(MaterialInsertDTO dto)
        {
            try 
            {
                var material = _mapper.Map<MaterialInsertDTO, Material>(dto);

                var result = _validator.ValidateBeforeInsert(material);

                if (!result.IsValid)
                    throw new ValidationException(result);

                var entity = await _repository.InsertAsync(material);

                return new Response<MaterialResponseDTO>(
                    _mapper.Map<Material, MaterialResponseDTO>(entity));
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

        public async Task<Response<MaterialResponseDTO>> UpdateAsync(long id, MaterialUpdateDTO dto)
        {
            try
            {
                var material = _mapper.Map<MaterialUpdateDTO, Material>(dto);

                material.Id = id;

                var result = _validator.ValidateBeforeUpdate(material);

                if (!result.IsValid)
                    throw new ValidationException(result);

                var entity = await _repository.UpdateAsync(id, material);

                return new Response<MaterialResponseDTO>(
                    _mapper.Map<Material, MaterialResponseDTO>(entity));
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

        public async Task<Response<MaterialResponseDTO>> AddSuppliersAsync(long materialId, MaterialAddSuppliersDTO dto)
        {
            try
            {
                var entity = await _repository.GetByIdAsync(materialId, p => p.Suppliers);

                if (entity is null)
                    throw new InfoException($"O {materialId} nao foi encontrado.", ErrorCodeEnum.NotFound);

                if (entity.Suppliers.Count() + dto.SupplierIds.Count() > 5)
                    throw new InfoException($"Limite de fornecedores será ultrapassado. Possuem {entity.Suppliers.Count()} já associados e o limite é 5 (cinco).", ErrorCodeEnum.NotFound);

                var addSuppliers = new List<MaterialSupplier>();
                var invalidSuppliers = new List<long>();

                var result = _validator.ValidateSuppliersAsync(dto.SupplierIds);

                if (!result.IsValid)
                    throw new ValidationException(result);

                foreach (var supplierId in dto.SupplierIds)
                {
                    addSuppliers.Add(new MaterialSupplier
                    {
                        MaterialId = materialId,
                        SupplierId = supplierId,
                    });
                }

                if (invalidSuppliers.Any())
                    return new Response<MaterialResponseDTO>(ErrorCodeEnum.NotFound, $"Os seguintes fornecedores são inválidos: {string.Join(",", invalidSuppliers)}");

                await _repository.AddSuppliersAsync(materialId, addSuppliers);

                return new Response<MaterialResponseDTO>();
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

        public async Task<Response<MaterialResponseDTO>> RemoveSuppliersAsync(long materialId, MaterialRemoveSuppliersDTO dto)
        {
            try
            {
                if (await _repository.GetByIdAsync(materialId) is null)
                    throw new InfoException($"O {materialId} nao foi encontrado.", ErrorCodeEnum.NotFound);

                var removeSuppliers = new List<MaterialSupplier>();

                foreach (var supplierId in dto.SupplierIds)
                {
                    var supplier = await _materialSupplierService.GetByIdAsync(supplierId);

                    removeSuppliers.Add(supplier);
                }

                await _repository.RemoveSuppliersAsync(materialId, removeSuppliers);

                return new Response<MaterialResponseDTO>();
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
