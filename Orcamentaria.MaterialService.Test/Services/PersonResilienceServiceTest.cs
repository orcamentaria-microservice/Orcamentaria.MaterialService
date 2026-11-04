using FluentAssertions;
using Moq;
using Moq.AutoMock;
using Orcamentaria.Lib.Domain.Models.Responses;
using Orcamentaria.Lib.Domain.Services;
using Orcamentaria.MaterialService.Domain.DTOs.Person;
using Orcamentaria.MaterialService.Application.Services;
using Orcamentaria.MaterialService.Test.Fixtures;
using Polly;
using Polly.Fallback;
using System.Text.Json;
using Xunit;

namespace Orcamentaria.MaterialService.Test.Services
{
    [Collection(nameof(MaterialCollection))]
    public class PersonResilienceServiceTest
    {
        private readonly MaterialFixture _fixture;
        private readonly AutoMocker _mocker;
        private readonly PersonResilienceService _service;

        public PersonResilienceServiceTest(MaterialFixture fixture)
        {
            _fixture = fixture;
            _mocker = new AutoMocker();
            _service = _mocker.CreateInstance<PersonResilienceService>(true);
        }

        [Fact]
        public void CreatePredicate_ReturnsNonNullPredicateBuilder()
        {
            var builder = _service.CreatePredicate();
            builder.Should().NotBeNull();
        }

        [Fact]
        public async Task FallbackAction_WhenCacheEmpty_ReturnsResponseWithIdsMapped()
        {
            var faker = _fixture.Faker;
            var supplierIds = new List<long> { faker.Random.Long(1, 1000), faker.Random.Long(1001, 2000) };

            var context = ResilienceContextPool.Shared.Get();
            context.Properties.Set(new ResiliencePropertyKey<IEnumerable<long>>("supplierIds"), supplierIds);

            _mocker.GetMock<ICacheService>()
                .Setup(m => m.GetAsync<string>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string?)null);

            var outcome = Outcome.FromResult<Response<IEnumerable<PersonResponseDTO>>>(new Response<IEnumerable<PersonResponseDTO>>());
            var args = new FallbackActionArguments<Response<IEnumerable<PersonResponseDTO>>>(context, outcome);

            var func = _service.FallbackAction();
            var resultOutcome = await func(args);
            var response = resultOutcome.Result;

            response.Should().NotBeNull();
            response.Success.Should().BeFalse();
            response.Message.Should().Contain("Fallback");
            response.Data.Should().NotBeNull();

            var ids = response.Data!.Select(p => p.Id).ToHashSet();
            ids.SetEquals(supplierIds).Should().BeTrue();

            ResilienceContextPool.Shared.Return(context);
        }

        [Fact]
        public async Task FallbackAction_WhenCacheHasEntries_ReturnsMergedDataFromCache()
        {
            var faker = _fixture.Faker;
            var supplierIds = new List<long> { faker.Random.Long(1, 1000), faker.Random.Long(1001, 2000) };

            var cachedSupplier = new PersonResponseDTO
            {
                Id = supplierIds[0],
                Name = faker.Person.FullName,
                IsFromCache = true
            };

            var cachedList = new List<PersonResponseDTO> { cachedSupplier };
            var cachedJson = JsonSerializer.Serialize(cachedList);

            var context = ResilienceContextPool.Shared.Get();
            context.Properties.Set(new ResiliencePropertyKey<IEnumerable<long>>("supplierIds"), supplierIds);

            _mocker.GetMock<ICacheService>()
                .Setup(m => m.GetAsync<string>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(cachedJson);

            var outcome = Outcome.FromResult<Response<IEnumerable<PersonResponseDTO>>>(new Response<IEnumerable<PersonResponseDTO>>());
            var args = new FallbackActionArguments<Response<IEnumerable<PersonResponseDTO>>>(context, outcome);

            var func = _service.FallbackAction();
            var resultOutcome = await func(args);
            var response = resultOutcome.Result;

            response.Should().NotBeNull();
            response.Success.Should().BeFalse();
            response.Message.Should().Contain("Fallback");
            response.Data.Should().NotBeNull();

            var list = response.Data!.ToList();
            var first = list.First(x => x.Id == supplierIds[0]);
            first.Name.Should().Be(cachedSupplier.Name);

            var second = list.First(x => x.Id == supplierIds[1]);
            second.Name.Should().BeNull();

            ResilienceContextPool.Shared.Return(context);
        }
    }
}
