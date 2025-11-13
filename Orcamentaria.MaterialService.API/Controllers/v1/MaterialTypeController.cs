using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orcamentaria.MaterialService.Domain.Services;
using Orcamentaria.Lib.Domain.Models;
using Orcamentaria.Lib.Domain.Models.Responses;
using Orcamentaria.MaterialService.Domain.DTOs.MaterialType;

namespace Orcamentaria.MaterialService.API.Controllers.v1
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class MaterialTypeController : Controller
    {
        private readonly IMaterialTypeService _service;

        public MaterialTypeController(IMaterialTypeService service)
        {
            _service = service;
        }

        [Authorize(Roles = "MASTER,MATERIAL:READ")]
        [HttpPost("Get", Name = "MaterialTypeGet")]
        public async Task<Response<IEnumerable<MaterialTypeResponseDTO>>?> GetAsync([FromBody] GridParams gridParams)
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
        [HttpPost("Insert", Name = "MaterialTypeInsert")]
        public async Task<Response<MaterialTypeResponseDTO>> InsertAsync([FromBody] MaterialTypeInsertDTO dto)
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
        [HttpPut("Update/{id}", Name = "MaterialTypeUpdate")]
        public async Task<Response<MaterialTypeResponseDTO>> UpdateAsync(long id, [FromBody] MaterialTypeUpdateDTO dto)
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
    }
}
