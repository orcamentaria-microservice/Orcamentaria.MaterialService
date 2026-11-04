using Microsoft.Extensions.DependencyInjection;
using Orcamentaria.Lib.Domain.Enums;
using Orcamentaria.Lib.Domain.Exceptions;
using Orcamentaria.Lib.Domain.Models.Responses;
using Orcamentaria.Lib.Domain.Services;
using Orcamentaria.MaterialService.Domain.DTOs.Person;
using Polly;
using Polly.CircuitBreaker;
using Polly.Fallback;
using System.Text.Json;

namespace Orcamentaria.MaterialService.Application.Services
{

    public class PersonResilienceService : IResilienceService<Response<IEnumerable<PersonResponseDTO>>>
    {
        private readonly ICacheService _cache;

        public PersonResilienceService([FromKeyedServices("memory")] ICacheService cache)
        {
            _cache = cache;
        }

        public PredicateBuilder<Response<IEnumerable<PersonResponseDTO>>> CreatePredicate()
            => new PredicateBuilder<Response<IEnumerable<PersonResponseDTO>>>()
            .Handle<ServiceUnavailableException>()
            .Handle<BrokenCircuitException>()
            .HandleResult(args => !args.Success && args.Error?.ErrorCode == ErrorCodeEnum.ServiceUnavailable);

        public Func<FallbackActionArguments<Response<IEnumerable<PersonResponseDTO>>>, ValueTask<Outcome<Response<IEnumerable<PersonResponseDTO>>>>> FallbackAction()
            => async args =>
            {
                args.Context.Properties.TryGetValue(new ResiliencePropertyKey<IEnumerable<long>>("supplierIds"), out var supplierIds);

                var suppliersCached = await _cache.GetAsync<string>("supplierKey");

                if (string.IsNullOrEmpty(suppliersCached))
                    return Outcome.FromResult(new Response<IEnumerable<PersonResponseDTO>>
                    {
                        Success = false,
                        Message = "Serviço inacessivel - Fallback",
                        Data = supplierIds?.Select(x => new PersonResponseDTO
                        {
                            Id = x,
                        })
                    });

                var suppliers = JsonSerializer.Deserialize<IEnumerable<PersonResponseDTO>>(suppliersCached);

                return Outcome.FromResult(new Response<IEnumerable<PersonResponseDTO>>
                {
                    Success = false,
                    Message = "Serviço inacessivel - Fallback",
                    Data = supplierIds?.Select(id =>
                                suppliers?.FirstOrDefault(x => x.Id == id) ??
                                    new PersonResponseDTO
                                    {
                                        Id = id,
                                    })
                });
            };
    }
}
