using Orcamentaria.MaterialService.Domain.Models;
using Orcamentaria.Lib.Domain.Models;
using Orcamentaria.Lib.Domain.Models.Responses;
using Orcamentaria.MaterialService.Domain.DTOs.MaterialType;

namespace Orcamentaria.MaterialService.Domain.Services
{
    public interface IMaterialTypeService
    {
        Task<MaterialType?> GetByIdAsync(long id);
        Task<Response<IEnumerable<MaterialTypeResponseDTO>>?> GetAsync(GridParams gridParams);
        Task<Response<MaterialTypeResponseDTO>> InsertAsync(MaterialTypeInsertDTO dto);
        Task<Response<MaterialTypeResponseDTO>> UpdateAsync(long id, MaterialTypeUpdateDTO dto);
    }
}
