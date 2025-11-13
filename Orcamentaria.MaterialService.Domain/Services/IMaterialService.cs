using Orcamentaria.MaterialService.Domain.Models;
using Orcamentaria.Lib.Domain.Models;
using Orcamentaria.Lib.Domain.Models.Responses;
using Orcamentaria.MaterialService.Domain.DTOs.Material;

namespace Orcamentaria.MaterialService.Domain.Services
{
    public interface IMaterialService
    {
        Task<Material?> GetByIdAsync(long id);
        Task<Response<IEnumerable<MaterialResponseDTO>>?> GetAsync(GridParams gridParams);
        Task<Response<MaterialResponseDTO>> InsertAsync(MaterialInsertDTO dto);
        Task<Response<MaterialResponseDTO>> UpdateAsync(long id, MaterialUpdateDTO dto);
        Task<Response<MaterialResponseDTO>> AddSuppliersAsync(long materialId, MaterialAddSuppliersDTO dto);
        Task<Response<MaterialResponseDTO>> RemoveSuppliersAsync(long materialId, MaterialRemoveSuppliersDTO dto);
    }
}
