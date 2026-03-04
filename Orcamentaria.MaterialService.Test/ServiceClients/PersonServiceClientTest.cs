using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Moq.AutoMock;
using Orcamentaria.Lib.Domain.Enums;
using Orcamentaria.Lib.Domain.Exceptions;
using Orcamentaria.Lib.Domain.Models.Configurations;
using Orcamentaria.Lib.Domain.Models.Exceptions;
using Orcamentaria.Lib.Domain.Models.Responses;
using Orcamentaria.Lib.Domain.Providers;
using Orcamentaria.Lib.Domain.Services;
using Orcamentaria.MaterialService.Domain.DTOs.Person;
using Orcamentaria.MaterialService.Infrastructure.ServiceClients;
using Orcamentaria.MaterialService.Test.Fixtures;
using Polly;
using System.Text.Json;
using Xunit;

namespace Orcamentaria.MaterialService.Test.Services
{
    [Collection(nameof(MaterialCollection))]
    public class PersonServiceClientTest
    {
        private readonly MaterialFixture _fixture;
        private readonly AutoMocker _mocker;
        private readonly PersonServiceClient _service;

        public PersonServiceClientTest(MaterialFixture fixture)
        {
            _fixture = fixture;
            _mocker = new AutoMocker();

            var optionsMock = new Mock<IOptions<ApiGetawayConfiguration>>();
            optionsMock.Setup(o => o.Value).Returns(new ApiGetawayConfiguration { BaseUrl = "http://base" });
            _mocker.Use(optionsMock.Object);
            var emptyPipeline = ResiliencePipeline<Response<IEnumerable<PersonResponseDTO>>>.Empty;
            _mocker.Use(emptyPipeline);

            _service = _mocker.CreateInstance<PersonServiceClient>();
        }

        #region GetSuppliersAsync
        [Fact]
        public async Task GetSuppliersAsync_WhenPipelineReturnsSuccessAndCacheEmpty_SetsCacheAndReturnsResponse()
        {
            var faker = _fixture.Faker;
            var ids = new HashSet<long> { faker.Random.Long(1, 1000), faker.Random.Long(1001, 2000) };
            var apiResponseList = ids.Select(id => new PersonResponseDTO { Id = id, Name = faker.Person.FullName }).ToList();
            var apiResponse = new Response<IEnumerable<PersonResponseDTO>>(apiResponseList);

            _mocker.GetMock<ITokenProvider>()
                .Setup(t => t.GetTokenAsync(It.IsAny<bool>()))
                .ReturnsAsync("token");

            _mocker.GetMock<IMemoryCacheService>()
                .Setup(m => m.GetMemoryCache(It.IsAny<string>(), out It.Ref<string?>.IsAny))
                .Returns((string key, out string? val) =>
                {
                    val = null;
                    return true;
                });

            _mocker.GetMock<IApiGetawayService>()
                .Setup(a => a.Routing<IEnumerable<PersonResponseDTO>>(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<IDictionary<string, string>?>(),
                    It.IsAny<object?>()))
                .ReturnsAsync(apiResponse);

            var response = await _service.GetSuppliersAsync(ids);

            response.Should().NotBeNull();
            response.Success.Should().BeTrue();
            response.Data.Should().NotBeNull();
            response.Data!.Select(x => x.Id).Should().BeEquivalentTo(ids);

            _mocker.GetMock<IMemoryCacheService>().Verify(m => m.SetMemoryCache(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetSuppliersAsync_WhenPipelineReturnsSuccessAndCacheHasValue_MergesAndSetsCache()
        {
            var faker = _fixture.Faker;
            var ids = new HashSet<long> { faker.Random.Long(1, 1000), faker.Random.Long(1001, 2000) };
            var apiResponseList = ids.Select(id => new PersonResponseDTO { Id = id, Name = faker.Person.FullName }).ToList();
            var apiResponse = new Response<IEnumerable<PersonResponseDTO>>(apiResponseList);
            var cached = new List<PersonResponseDTO> { new PersonResponseDTO { Id = apiResponseList.First().Id, Name = "cached-name" } };
            var cachedJson = JsonSerializer.Serialize(cached);

            _mocker.GetMock<ITokenProvider>()
                .Setup(t => t.GetTokenAsync(It.IsAny<bool>()))
                .ReturnsAsync("token");

            _mocker.GetMock<IMemoryCacheService>()
                .Setup(m => m.GetMemoryCache(It.IsAny<string>(), out It.Ref<string?>.IsAny))
                .Returns((string key, out string? val) =>
                {
                    val = cachedJson;
                    return true;
                });

            _mocker.GetMock<IApiGetawayService>()
                .Setup(a => a.Routing<IEnumerable<PersonResponseDTO>>(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<IDictionary<string, string>?>(),
                    It.IsAny<object?>()))
                .ReturnsAsync(apiResponse);

            var response = await _service.GetSuppliersAsync(ids);

            response.Should().NotBeNull();
            response.Success.Should().BeTrue();
            response.Data.Should().NotBeNull();

            _mocker.GetMock<IMemoryCacheService>().Verify(m => m.SetMemoryCache(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetSuppliersAsync_WhenPipelineReturnsNotSuccess_ReturnsSameResponseAndDoesNotSetCache()
        {
            var faker = _fixture.Faker;
            var ids = new HashSet<long> { faker.Random.Long(1, 1000) };
            var apiResponse = new Response<IEnumerable<PersonResponseDTO>> { Success = false, Error = new ResponseError(ErrorCodeEnum.NotFound) };

            _mocker.GetMock<ITokenProvider>()
                .Setup(t => t.GetTokenAsync(It.IsAny<bool>()))
                .ReturnsAsync("token");

            _mocker.GetMock<IApiGetawayService>()
                .Setup(a => a.Routing<IEnumerable<PersonResponseDTO>>(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<IDictionary<string, string>?>(),
                    It.IsAny<object?>()))
                .ReturnsAsync(apiResponse);

            var result = await _service.GetSuppliersAsync(ids);

            result.Should().BeSameAs(apiResponse);

            _mocker.GetMock<IMemoryCacheService>().Verify(m => m.SetMemoryCache(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetSuppliersAsync_WhenTokenProviderThrowsBusinessException_ThrowsBusinessException()
        {
            var faker = _fixture.Faker;
            var ids = new HashSet<long> { faker.Random.Long(1, 1000) };

            _mocker.GetMock<ITokenProvider>()
                .Setup(t => t.GetTokenAsync(It.IsAny<bool>()))
                .ThrowsAsync(new Exception("boom"));

            Func<Task> act = async () => await _service.GetSuppliersAsync(ids);

            var ex = await act.Should().ThrowAsync<BusinessException>();
            ex.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.InternalError);
        }

        [Fact]
        public async Task GetSuppliersAsync_WhenApiGetawayThrowsDatabaseException_Propagates()
        {
            var faker = _fixture.Faker;
            var ids = new HashSet<long> { faker.Random.Long(1, 1000) };

            _mocker.GetMock<ITokenProvider>().Setup(t => t.GetTokenAsync(It.IsAny<bool>())).ReturnsAsync("token");

            _mocker.GetMock<IApiGetawayService>()
                .Setup(a => a.Routing<IEnumerable<PersonResponseDTO>>(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<IDictionary<string, string>?>(),
                    It.IsAny<object?>()))
                .ThrowsAsync(new DatabaseException("db"));

            Func<Task> act = async () => await _service.GetSuppliersAsync(ids);

            var ex = await act.Should().ThrowAsync<DatabaseException>();
            ex.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.DatabaseError);
        }
        #endregion
    }
}
