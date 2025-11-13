using Microsoft.Extensions.Options;
using Orcamentaria.Lib.Domain.Exceptions;
using Orcamentaria.Lib.Domain.Models;
using Orcamentaria.Lib.Domain.Models.Configurations;
using Orcamentaria.Lib.Domain.Models.Responses;
using Orcamentaria.Lib.Domain.Providers;
using Orcamentaria.Lib.Domain.Services;
using Orcamentaria.MaterialService.Domain.DTOs.Person;
using Orcamentaria.MaterialService.Domain.ServiceClient;
using Polly;
using System.Text.Json;

namespace Orcamentaria.MaterialService.Infrastructure.ServiceClient
{
    public class PersonServiceClient : IPersonServiceClient
    {
        private readonly IApiGetawayService _apiGetawayService;
        private readonly ApiGetawayConfiguration _apiGetawayConfiguration;
        private readonly ITokenProvider _tokenProvider;
        private readonly ResiliencePipeline<Response<IEnumerable<PersonResponseDTO>>> _pipeline;
        private readonly IMemoryCacheService _memoryCacheService;

        public PersonServiceClient(
            IApiGetawayService apiGetawayService, 
            IOptions<ApiGetawayConfiguration> apiGetawayConfiguration,
            ITokenProvider tokenProvider,
            ResiliencePipeline<Response<IEnumerable<PersonResponseDTO>>> pipeline,
            IMemoryCacheService memoryCacheService)
        {
            _apiGetawayService = apiGetawayService;
            _apiGetawayConfiguration = apiGetawayConfiguration.Value;
            _tokenProvider = tokenProvider;
            _pipeline = pipeline;
            _memoryCacheService = memoryCacheService;
        }

        public async Task<Response<IEnumerable<PersonResponseDTO>>> GetSuppliersAsync(IEnumerable<long> personIds)
        {
            try
            {
                var gridParams = new GridParams
                {
                    Filters = new List<FilterParam>
                    {
                        new FilterParam
                        {
                            Field = "id",
                            Operator = "in",
                            Value = personIds.ToArray()
                        },
                        new FilterParam
                        {
                            Field = "active",
                            Operator = "eq",
                            Value = true
                        },
                        new FilterParam
                        {
                            Field = "type",
                            Operator = "eq",
                            Value = 2
                        }
                    }
                };

                var token = await _tokenProvider.GetTokenAsync();
                ResilienceContext context = ResilienceContextPool.Shared.Get(continueOnCapturedContext: true);
                context.Properties.Set(new ResiliencePropertyKey<IEnumerable<long>>("supplierIds"), personIds);

                var response = await _pipeline.ExecuteAsync(
                    async _ =>
                    {
                        return await _apiGetawayService.Routing<IEnumerable<PersonResponseDTO>>(
                            baseUrl: _apiGetawayConfiguration.BaseUrl,
                            serviceName: "PersonService",
                            endpointName: "PersonGetForService",
                            token,
                            @params: null,
                            payload: gridParams
                            );
                    }, context);

                if (!response.Success)
                    return response;

                string key = "supplierKey";
                _memoryCacheService.GetMemoryCache(key, out string? suppliersCached);

                if (string.IsNullOrEmpty(suppliersCached))
                {
                    _memoryCacheService.SetMemoryCache(key, JsonSerializer.Serialize(
                        response.Data!.Select(x => new PersonResponseDTO
                        {
                            Id = x.Id,
                            Name = x.Name,
                            IsFromCache = true
                        })
                    ));

                    return response;
                }

                var suppliers = JsonSerializer.Deserialize<IEnumerable<PersonResponseDTO>>(suppliersCached);

                _memoryCacheService.SetMemoryCache(key, JsonSerializer.Serialize(
                    suppliers!.Select(x =>  response.Data?.FirstOrDefault(u => u.Id == x.Id) ?? x)
                        .Select(x => new PersonResponseDTO
                        {
                            Id = x.Id,
                            Name = x.Name,
                            IsFromCache = true
                        })
                    )
                );
                
                return response;
            }
            catch (Exception ex)
            {
                throw new UnexpectedException($"Erro inesperado na consulta de fornecedores: {ex.Message}", ex);
            }
        }
    }
}
