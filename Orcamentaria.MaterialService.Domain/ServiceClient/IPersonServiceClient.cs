using Orcamentaria.Lib.Domain.Models.Responses;
using Orcamentaria.MaterialService.Domain.DTOs.Person;

namespace Orcamentaria.MaterialService.Domain.ServiceClient
{
    public interface IPersonServiceClient
    {
        Task<Response<IEnumerable<PersonResponseDTO>>> GetSuppliersAsync(IEnumerable<long> personIds);
    }
}
