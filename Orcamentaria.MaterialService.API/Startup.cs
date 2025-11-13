using Orcamentaria.Lib.Domain.Models.Resilience;
using Orcamentaria.Lib.Domain.Models.Responses;
using Orcamentaria.Lib.Domain.Validators;
using Orcamentaria.Lib.Infrastructure.Configures;
using Orcamentaria.MaterialService.Application.Services;
using Orcamentaria.MaterialService.Application.Validators;
using Orcamentaria.MaterialService.Domain.DTOs.Person;
using Orcamentaria.MaterialService.Domain.Mappers;
using Orcamentaria.MaterialService.Domain.Models;
using Orcamentaria.MaterialService.Domain.Repositories;
using Orcamentaria.MaterialService.Domain.ServiceClient;
using Orcamentaria.MaterialService.Domain.Services;
using Orcamentaria.MaterialService.Domain.Validators;
using Orcamentaria.MaterialService.Infrastructure.Contexts;
using Orcamentaria.MaterialService.Infrastructure.Repositories;
using Orcamentaria.MaterialService.Infrastructure.ServiceClient;

namespace Orcamentaria.MaterialService.API
{
    public class Startup
    {
        private readonly string _serviceName = "Orcamentaria.MaterialService";
        private readonly string _apiVersion = "v1";
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; set; }

        public void ConfigureServices(IServiceCollection services)
        {
            Configuration = services.ResolveConfigs(Configuration, _serviceName);

            services.AddServiceRegistryHosted(Configuration);

            services.ResolveCommonServicesWithMySql<MySqlContext>(
                configuration: Configuration,
                serviceName: _serviceName,
                apiVersion: _apiVersion,
                customServices: () =>
                {
                    //Mappers
                    services.AddAutoMapper(_ => { },
                        typeof(MaterialMapper),
                        typeof(MaterialTypeMapper));

                    //Repositories
                    services.AddScoped<IMaterialRepository, MaterialRespository>();
                    services.AddScoped<IMaterialTypeRepository, MaterialTypeRespository>();
                    services.AddScoped<IMaterialSupplierRepository, MaterialSupplierRespository>();

                    //Services
                    services.AddScoped<IMaterialService, Application.Services.MaterialService>();
                    services.AddScoped<IMaterialTypeService, MaterialTypeService>();
                    services.AddScoped<IMaterialSupplierService, MaterialSupplierService>();

                    //Validators
                    services.AddScoped<IMaterialValidator, MaterialValidator>();
                    services.AddScoped<IValidatorEntity<MaterialType>, MaterialTypeValidator>();

                    //ServiceClients
                    services.AddScoped<IPersonServiceClient, PersonServiceClient>();

                    //Resiliences
                    services.ConfigureResilience<Response<IEnumerable<PersonResponseDTO>>, PersonResilienceService>(
                    retryResilience: new RetryResilience(),
                    circuitBreakerResilience: new CircuitBreakerResilience<Response<IEnumerable<PersonResponseDTO>>>());
                });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
            => app.ConfigureCommon(env, _serviceName, _apiVersion);
    }
}
