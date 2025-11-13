using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orcamentaria.MaterialService.Domain.DTOs.Material;
using Orcamentaria.MaterialService.Domain.Services;
using Orcamentaria.Lib.Domain.Models;
using Orcamentaria.Lib.Domain.Models.Responses;

namespace Orcamentaria.MaterialService.API.Controllers.v1
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class MaterialController : Controller
    {
        private readonly IMaterialService _service;

        public MaterialController(IMaterialService service)
        {
            _service = service;
        }

        [Authorize(Roles = "MASTER,MATERIAL:READ")]
        [HttpPost("Get", Name = "MaterialGet")]
        public async Task<Response<IEnumerable<MaterialResponseDTO>>?> GetAsync([FromBody] GridParams gridParams)
        {
            try
            {
                return await _service.GetAsync(gridParams);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [Authorize(Roles = "MASTER,MATERIAL:INSERT")]
        [HttpPost("Insert", Name = "MaterialInsert")]
        public async Task<Response<MaterialResponseDTO>> InsertAsync([FromBody] MaterialInsertDTO dto)
        {
            try
            {
                return await _service.InsertAsync(dto);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [Authorize(Roles = "MASTER,MATERIAL:UPDATE")]
        [HttpPut("Update/{id}", Name = "MaterialUpdate")]
        public async Task<Response<MaterialResponseDTO>> UpdateAsync(long id, [FromBody] MaterialUpdateDTO dto)
        {
            try
            {
                return await _service.UpdateAsync(id, dto);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [Authorize(Roles = "MASTER,MATERIAL:UPDATE")]
        [HttpPut("AddSuppliers/{id}", Name = "UserAddSuppliers")]
        public async Task<Response<MaterialResponseDTO>> AddSuppliersAsync(long id, [FromBody] MaterialAddSuppliersDTO dto)
        {
            try
            {
                return await _service.AddSuppliersAsync(id, dto);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [Authorize(Roles = "MASTER,MATERIAL:UPDATE")]
        [HttpPut("RemoveSuppliers/{id}", Name = "UserRemoveSuppliers")]
        public async Task<Response<MaterialResponseDTO>> RemoveSuppliersAsync(long id, [FromBody] MaterialRemoveSuppliersDTO dto)
        {
            try
            {
                return await _service.RemoveSuppliersAsync(id, dto);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
