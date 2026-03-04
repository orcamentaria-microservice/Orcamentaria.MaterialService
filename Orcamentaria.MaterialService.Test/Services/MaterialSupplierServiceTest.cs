using FluentAssertions;
using Moq;
using Moq.AutoMock;
using Orcamentaria.Lib.Domain.Enums;
using Orcamentaria.Lib.Domain.Exceptions;
using Orcamentaria.Lib.Domain.Models;
using Orcamentaria.Lib.Domain.Models.Exceptions;
using Orcamentaria.Lib.Domain.Models.Responses;
using Orcamentaria.MaterialService.Application.Services;
using Orcamentaria.MaterialService.Domain.Models;
using Orcamentaria.MaterialService.Domain.Repositories;
using Orcamentaria.MaterialService.Test.Fixtures;
using Xunit;

namespace Orcamentaria.MaterialService.Test.Services
{
    [Collection(nameof(MaterialSupplierCollection))]
    public class MaterialSupplierServiceTest
    {
        private readonly MaterialSupplierFixture _fixture;
        private readonly AutoMocker _mocker;
        private readonly MaterialSupplierService _service;

        public MaterialSupplierServiceTest(MaterialSupplierFixture fixture)
        {
            _fixture = fixture;
            _mocker = new AutoMocker();
            _service = _mocker.CreateInstance<MaterialSupplierService>(true);
        }

        #region GetByIdAsync

        [Xunit.Theory]
        [InlineData(1)]
        [InlineData(5)]
        public async Task GetByIdAsync_WhenHaveData_ReturnsData(long id)
        {
            var entity = _fixture.CreateEntity(id);

            _mocker.GetMock<IMaterialSupplierRepository<MaterialSupplier>>()
                .Setup(r => r.GetByIdAsync(It.Is<long>(p => p == id)))
                .ReturnsAsync(entity);

            var result = await _service.GetByIdAsync(id);

            result.Should().NotBeNull();
            result.Should().BeSameAs(entity);

            _mocker.GetMock<IMaterialSupplierRepository<MaterialSupplier>>()
                .Verify(r => r.GetByIdAsync(It.Is<long>(p => p == id)), Times.Once);
        }

        [Xunit.Theory]
        [InlineData(0)]
        public async Task GetByIdAsync_WhenNotHaveData_ReturnsNull(long id)
        {
            _mocker.GetMock<IMaterialSupplierRepository<MaterialSupplier>>()
                .Setup(r => r.GetByIdAsync(It.Is<long>(p => p == id)))
                .ReturnsAsync((MaterialSupplier?)null);

            var result = await _service.GetByIdAsync(id);

            result.Should().BeNull();

            _mocker.GetMock<IMaterialSupplierRepository<MaterialSupplier>>()
                .Verify(r => r.GetByIdAsync(It.Is<long>(p => p == id)), Times.Once);
        }

        [Xunit.Theory]
        [InlineData(99)]
        public async Task GetByIdAsync_WhenRepositoryThrowsDatabaseException_Propagates(long id)
        {
            var repoEx = new DatabaseException("db error");

            _mocker.GetMock<IMaterialSupplierRepository<MaterialSupplier>>()
                .Setup(r => r.GetByIdAsync(It.IsAny<long>()))
                .ThrowsAsync(repoEx);

            Func<Task> act = async () => await _service.GetByIdAsync(id);

            var ex = await act.Should().ThrowAsync<DatabaseException>();
            ex.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.DatabaseError);

            _mocker.GetMock<IMaterialSupplierRepository<MaterialSupplier>>()
                .Verify(r => r.GetByIdAsync(It.IsAny<long>()), Times.Once);
        }

        #endregion

        #region GetAsync

        [Xunit.Fact]
        public async Task GetAsync_WhenHaveData_ReturnsDataList()
        {
            var faker = _fixture.Faker;
            var gridParams = _fixture.CreateGridParamsWithWithoutFilter();

            var list = new List<MaterialSupplier>
            {
                _fixture.CreateEntity(faker.Random.Long(1, 9999)),
                _fixture.CreateEntity(faker.Random.Long(10000, 19999))
            };

            var repoResponse = (list.AsEnumerable(), new ResponsePagination(1, 10, list.Count));

            _mocker.GetMock<IMaterialSupplierRepository<MaterialSupplier>>()
                .Setup(r => r.GetAsync(It.Is<GridParams>(g => g == gridParams)))
                .ReturnsAsync(repoResponse);

            var result = await _service.GetAsync(gridParams);

            result.Should().NotBeNull();
            result.Should().HaveCount(list.Count());
            result.Should().BeEquivalentTo(list);

            _mocker.GetMock<IMaterialSupplierRepository<MaterialSupplier>>()
                .Verify(r => r.GetAsync(It.Is<GridParams>(g => g == gridParams)), Times.Once);
        }

        [Xunit.Fact]
        public async Task GetAsync_WhenNotHaveData_ThrowsInfoException()
        {
            var gridParams = _fixture.CreateGridParamsWithWithoutFilter();
            var repoResponse = (Enumerable.Empty<MaterialSupplier>(), new ResponsePagination(1, 10, 0));

            _mocker.GetMock<IMaterialSupplierRepository<MaterialSupplier>>()
                .Setup(r => r.GetAsync(It.IsAny<GridParams>()))
                .ReturnsAsync(repoResponse);

            Func<Task> act = async () => await _service.GetAsync(gridParams);

            var ex = await act.Should().ThrowAsync<InfoException>();
            ex.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.NotFound);

            _mocker.GetMock<IMaterialSupplierRepository<MaterialSupplier>>()
                .Verify(r => r.GetAsync(It.Is<GridParams>(g => g == gridParams)), Times.Once);
        }

        [Xunit.Fact]
        public async Task GetAsync_WhenRepositoryThrowsDatabaseException_Propagates()
        {
            var gridParams = _fixture.CreateGridParamsWithWithoutFilter();
            var repoEx = new DatabaseException("db error");

            _mocker.GetMock<IMaterialSupplierRepository<MaterialSupplier>>()
                .Setup(r => r.GetAsync(It.IsAny<GridParams>()))
                .ThrowsAsync(repoEx);

            Func<Task> act = async () => await _service.GetAsync(gridParams);

            var ex = await act.Should().ThrowAsync<DatabaseException>();
            ex.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.DatabaseError);

            _mocker.GetMock<IMaterialSupplierRepository<MaterialSupplier>>()
                .Verify(r => r.GetAsync(It.Is<GridParams>(g => g == gridParams)), Times.Once);
        }

        #endregion
    }
}
