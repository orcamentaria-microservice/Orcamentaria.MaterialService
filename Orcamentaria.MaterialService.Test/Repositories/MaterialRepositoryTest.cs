using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Orcamentaria.Lib.Domain.Enums;
using Orcamentaria.Lib.Domain.Exceptions;
using Orcamentaria.Lib.Infrastructure.Contexts;
using Orcamentaria.Lib.Test.Repositories;
using Orcamentaria.MaterialService.Domain.Models;
using Orcamentaria.MaterialService.Infrastructure.Repositories;
using Orcamentaria.MaterialService.Test.Contexts;
using Orcamentaria.MaterialService.Test.Fixtures;
using Xunit;

namespace Orcamentaria.MaterialService.Test.Repositories
{
    [Collection(nameof(MaterialCollection))]
    public class MaterialRepositoryTest
    {
        private readonly MaterialFixture _fixture;
        private readonly MySqlContextTest _dbContext;
        private readonly UserAuthContext _userAuthContext;
        private readonly DbContextOptions<DbContext> _options;

        public MaterialRepositoryTest(MaterialFixture fixture)
        {
            _fixture = fixture;

            _userAuthContext = _fixture.CreateUserAuthContext();

            _options = new DbContextOptionsBuilder<DbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

            _dbContext = new MySqlContextTest(_options);
        }

        #region AddSuppliersAsync
        [Fact]
        public async Task AddSuppliersAsync_WhenParametersOK_ReturnsData()
        {
            var materialId = 1;
            var suppliers = new[] { new MaterialSupplier(), new MaterialSupplier(), new MaterialSupplier() };
            await _fixture.SeedInMemoryDatabase(_dbContext, materialId);

            var mockRepository = new Mock<MaterialRepository>(_dbContext, _userAuthContext) { CallBase = true };

            var result = await mockRepository.Object.AddSuppliersAsync(materialId, suppliers);

            result.Id.Should().Be(materialId);
            _dbContext.Materials.First(x => x.Id == materialId).Suppliers.Should().HaveCount(suppliers.Count());
        }

        [Fact]
        public async Task AddSuppliersAsync_WhenDatabaseIsEmpty_ReturnsData()
        {
            var materialId = 1;
            var suppliers = new[] { new MaterialSupplier(), new MaterialSupplier(), new MaterialSupplier() };

            var mockRepository = new Mock<MaterialRepository>(_dbContext, _userAuthContext) { CallBase = true };

            Func<Task> act = async () => await mockRepository.Object.AddSuppliersAsync(materialId, suppliers);

            var exception = await act.Should().ThrowAsync<DatabaseException>();
            exception.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.DatabaseError);
        }

        [Fact]
        public async Task AddSuppliersAsync_WhenMaterialIdNotFound_ThrowsDatabaseException()
        {
            var materialId = 4;
            var suppliers = new[] { new MaterialSupplier(), new MaterialSupplier(), new MaterialSupplier() };
            await _fixture.SeedInMemoryDatabase(_dbContext, 1);

            var mockRepository = new Mock<MaterialRepository>(_dbContext, _userAuthContext) { CallBase = true };

            Func<Task> act = async () => await mockRepository.Object.AddSuppliersAsync(materialId, suppliers);

            var exception = await act.Should().ThrowAsync<DatabaseException>();
            exception.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.DatabaseError);
        }

        [Fact]
        public async Task AddSuppliersAsync_WhenSuppliersParameterIsNull_ThrowsDatabaseException()
        {
            var materialId = 1;
            IEnumerable<MaterialSupplier> suppliers = null;
            await _fixture.SeedInMemoryDatabase(_dbContext, materialId);

            var mockRepository = new Mock<MaterialRepository>(_dbContext, _userAuthContext) { CallBase = true };

            Func<Task> act = async () => await mockRepository.Object.AddSuppliersAsync(materialId, suppliers);

            var exception = await act.Should().ThrowAsync<DatabaseException>();
            exception.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.DatabaseError);
        }

        [Fact]
        public async Task AddSuppliersAsync_WhenCompanyMismatch_ThrowsDatabaseException()
        {
            var userAuthContext = _fixture.CreateUserAuthContext();
            userAuthContext.CompanyId = 99999;
            var materialId = 1;
            var suppliers = new[] { new MaterialSupplier(), new MaterialSupplier(), new MaterialSupplier() };
            await _fixture.SeedInMemoryDatabase(_dbContext, materialId);

            var mockRepository = new Mock<MaterialRepository>(_dbContext, userAuthContext) { CallBase = true };

            Func<Task> act = async () => await mockRepository.Object.AddSuppliersAsync(materialId, suppliers);

            var exception = await act.Should().ThrowAsync<DatabaseException>();
            exception.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.DatabaseError);
        }
        #endregion

        #region RemoveSuppliersAsync
        [Fact]
        public async Task RemoveSuppliersAsync_WhenParametersOK_ReturnsData()
        {
            var materialId = 1;
            var supplierRemove = new MaterialSupplier { MaterialId = materialId, SupplierId = 1 };
            var suppliers = new[] { supplierRemove };
            await _fixture.SeedInMemoryDatabase(_dbContext, materialId);
            await _fixture.AddSuppliers(_dbContext, materialId, new[] { supplierRemove, new MaterialSupplier { MaterialId = materialId, SupplierId = 2 } });

            var mockRepository = new Mock<MaterialRepository>(_dbContext, _userAuthContext) { CallBase = true };

            var result = await mockRepository.Object.RemoveSuppliersAsync(materialId, suppliers);

            result.Id.Should().Be(materialId);
            result.Suppliers.FirstOrDefault(x => x.SupplierId == supplierRemove.SupplierId).Should().BeNull();
            _dbContext.Materials.First(x => x.Id == materialId).Suppliers.FirstOrDefault(x => x.SupplierId == supplierRemove.SupplierId).Should().BeNull();
        }

        [Fact]
        public async Task RemoveSuppliersAsync_WhenDatabaseIsEmpty_ReturnsData()
        {
            var materialId = 1;
            var supplierRemove = new MaterialSupplier { MaterialId = 1, SupplierId = 1 };
            var suppliers = new[] { supplierRemove };

            var mockRepository = new Mock<MaterialRepository>(_dbContext, _userAuthContext) { CallBase = true };

            Func<Task> act = async () => await mockRepository.Object.RemoveSuppliersAsync(materialId, suppliers);

            var exception = await act.Should().ThrowAsync<DatabaseException>();
            exception.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.DatabaseError);
        }

        [Fact]
        public async Task RemoveSuppliersAsync_WhenMaterialIdNotFound_ThrowsDatabaseException()
        {
            var materialId = 4;
            var supplierRemove = new MaterialSupplier { MaterialId = 4, SupplierId = 1 };
            var suppliers = new[] { supplierRemove };
            await _fixture.SeedInMemoryDatabase(_dbContext, 1);
            await _fixture.AddSuppliers(_dbContext, 1, new[] { new MaterialSupplier { MaterialId = 1, SupplierId = 2 } });

            var mockRepository = new Mock<MaterialRepository>(_dbContext, _userAuthContext) { CallBase = true };

            Func<Task> act = async () => await mockRepository.Object.RemoveSuppliersAsync(materialId, suppliers);

            var exception = await act.Should().ThrowAsync<DatabaseException>();
            exception.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.DatabaseError);
        }

        [Fact]
        public async Task RemoveSuppliersAsync_WhenSuppliersParameterIsNull_ThrowsDatabaseException()
        {
            var materialId = 1;
            IEnumerable<MaterialSupplier> suppliers = null;
            await _fixture.SeedInMemoryDatabase(_dbContext, materialId);
            await _fixture.AddSuppliers(_dbContext, materialId, new[] { new MaterialSupplier { MaterialId = materialId, SupplierId = 1 } });

            var mockRepository = new Mock<MaterialRepository>(_dbContext, _userAuthContext) { CallBase = true };

            Func<Task> act = async () => await mockRepository.Object.AddSuppliersAsync(materialId, suppliers);

            var exception = await act.Should().ThrowAsync<DatabaseException>();
            exception.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.DatabaseError);
        }

        [Fact]
        public async Task RemoveSuppliersAsync_WhenCompanyMismatch_ThrowsDatabaseException()
        {
            var userAuthContext = _fixture.CreateUserAuthContext();
            userAuthContext.CompanyId = 99999;
            var materialId = 1;
            var supplierRemove = new MaterialSupplier { MaterialId = materialId, SupplierId = 1 };
            var suppliers = new[] { supplierRemove };
            await _fixture.SeedInMemoryDatabase(_dbContext, materialId);
            await _fixture.AddSuppliers(_dbContext, materialId, new[] { supplierRemove, new MaterialSupplier { MaterialId = materialId, SupplierId = 2 } });

            var mockRepository = new Mock<MaterialRepository>(_dbContext, userAuthContext) { CallBase = true };

            Func<Task> act = async () => await mockRepository.Object.RemoveSuppliersAsync(materialId, suppliers);

            var exception = await act.Should().ThrowAsync<DatabaseException>();
            exception.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.DatabaseError);
        }
        #endregion
    }

    [Collection(nameof(MaterialCollection))]
    public class MaterialReadRepositoryTest : ReadWithCompanyRepositoryTests<Material, MySqlContextTest>
    {
        public MaterialReadRepositoryTest(MaterialFixture fixture) : base(fixture) { }
    }

    [Collection(nameof(MaterialCollection))]
    public class MaterialWriteRepositoryTest : WriteWithCompanyRepositoryTests<Material, MySqlContextTest>
    {
        public MaterialWriteRepositoryTest(MaterialFixture fixture) : base(fixture) { }
    }
}
