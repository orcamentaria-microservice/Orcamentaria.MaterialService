using AutoMapper;
using FluentAssertions;
using FluentValidation.Results;
using Moq;
using Moq.AutoMock;
using Orcamentaria.Lib.Domain.Enums;
using Orcamentaria.Lib.Domain.Exceptions;
using Orcamentaria.Lib.Domain.Models;
using Orcamentaria.Lib.Domain.Models.Exceptions;
using Orcamentaria.Lib.Domain.Models.Responses;
using Orcamentaria.MaterialService.Domain.DTOs.Material;
using Orcamentaria.MaterialService.Domain.DTOs.Person;
using Orcamentaria.MaterialService.Domain.Models;
using Orcamentaria.MaterialService.Domain.Repositories;
using Orcamentaria.MaterialService.Domain.ServiceClient;
using Orcamentaria.MaterialService.Domain.Services;
using Orcamentaria.MaterialService.Domain.Validators;
using Orcamentaria.MaterialService.Test.Fixtures;
using System.Linq.Expressions;
using Xunit;

namespace Orcamentaria.MaterialService.Test.Services
{
    [Collection(nameof(MaterialCollection))]
    public class MaterialServiceTest
    {
        private readonly MaterialFixture _fixture;
        private readonly AutoMocker _mocker;
        private readonly Application.Services.MaterialService _service;

        public MaterialServiceTest(MaterialFixture fixture)
        {
            _fixture = fixture;
            _mocker = new AutoMocker();
            _service = _mocker.CreateInstance<Application.Services.MaterialService>(true);
        }

        #region GetByIdAsync

        [Xunit.Theory]
        [InlineData(1)]
        [InlineData(4)]
        public async Task GetByIdAsync_WhenHaveData_ReturnsData(long id)
        {
            var repoResponse = _fixture.CreateEntity(id);
            _mocker.GetMock<IMaterialRepository<Material>>()
                .Setup(r => r.GetByIdAsync(It.Is<long>(p => p == id), It.IsAny<Expression<Func<Material, object>>[]>()))
                .ReturnsAsync(repoResponse);

            var result = await _service.GetByIdAsync(id);

            result.Should().NotBeNull();
            result.Should().BeSameAs(repoResponse);

            _mocker.GetMock<IMaterialRepository<Material>>()
                .Verify(r => r.GetByIdAsync(It.Is<long>(p => p == id), It.IsAny<Expression<Func<Material, object>>[]>()), Times.Once);
        }

        [Xunit.Theory]
        [InlineData(0)]
        public async Task GetByIdAsync_WhenNotHaveData_ReturnsNull(long id)
        {
            _mocker.GetMock<IMaterialRepository<Material>>()
                .Setup(r => r.GetByIdAsync(It.Is<long>(p => p == id), It.IsAny<Expression<Func<Material, object>>[]>()))
                .ReturnsAsync((Material)null);

            var result = await _service.GetByIdAsync(id);

            result.Should().BeNull();

            _mocker.GetMock<IMaterialRepository<Material>>()
                .Verify(r => r.GetByIdAsync(It.Is<long>(p => p == id), It.IsAny<Expression<Func<Material, object>>[]>()), Times.Once);
        }

        [Xunit.Theory]
        [InlineData(99)]
        public async Task GetByIdAsync_WhenRepositoryThrowsDatabaseException_Propagates(long id)
        {
            var repoEx = new DatabaseException("db error");
            _mocker.GetMock<IMaterialRepository<Material>>()
                .Setup(r => r.GetByIdAsync(It.IsAny<long>(), It.IsAny<Expression<Func<Material, object>>[]>()))
                .ThrowsAsync(repoEx);

            Func<Task> act = async () => await _service.GetByIdAsync(id);

            var ex = await act.Should().ThrowAsync<DatabaseException>();
            ex.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.DatabaseError);
        }

        #endregion

        #region GetAsync

        [Xunit.Fact]
        public async Task GetAsync_WhenHaveData_FillsSuppliers_FromPersonClient()
        {
            var faker = _fixture.Faker;
            var gridParams = _fixture.CreateGridParamsWithWithoutFilter();

            var materialId = faker.Random.Long(10, 9999);
            var supplierIdA = faker.Random.Long(100, 999);
            var supplierIdB = faker.Random.Long(1000, 1999);

            var material = new Material
            {
                Id = materialId,
                Name = faker.Commerce.ProductName(),
                Suppliers = new List<MaterialSupplier>
                {
                    new MaterialSupplier { MaterialId = materialId, SupplierId = supplierIdA },
                    new MaterialSupplier { MaterialId = materialId, SupplierId = supplierIdB }
                }
            };

            var repoResponse = (new List<Material> { material }, new ResponsePagination(1, 10, 1));

            var mappedDto = new MaterialResponseDTO { Id = material.Id, Name = material.Name, Suppliers = new List<PersonResponseDTO>() };

            var clientSuppliers = new List<PersonResponseDTO>
            {
                new PersonResponseDTO { Id = supplierIdA, Name = faker.Person.FullName, Active = true },
                new PersonResponseDTO { Id = supplierIdB, Name = faker.Person.FullName, Active = true },
                new PersonResponseDTO { Id = faker.Random.Long(2000,3000), Name = faker.Person.FullName, Active = true }
            };

            _mocker.GetMock<IMaterialRepository<Material>>()
                .Setup(r => r.GetAsync(It.Is<GridParams>(g => g == gridParams), It.IsAny<Expression<Func<Material, object>>[]>()))
                .ReturnsAsync(repoResponse);

            _mocker.GetMock<IMapper>()
                .Setup(m => m.Map<Material, MaterialResponseDTO>(It.IsAny<Material>()))
                .Returns(mappedDto);

            _mocker.GetMock<IPersonServiceClient>()
                .Setup(c => c.GetSuppliersAsync(It.IsAny<IEnumerable<long>>()))
                .ReturnsAsync(new Response<IEnumerable<PersonResponseDTO>>(clientSuppliers));

            var response = await _service.GetAsync(gridParams);

            response.Should().NotBeNull();
            response.Success.Should().BeTrue();
            var list = response!.Data!.ToList();
            list.Should().HaveCount(1);
            list[0].Suppliers.Should().HaveCount(2);
            list[0].Suppliers.Select(s => s.Id).Should().BeEquivalentTo(new[] { supplierIdA, supplierIdB });

            _mocker.GetMock<IMaterialRepository<Material>>()
                .Verify(r => r.GetAsync(It.Is<GridParams>(g => g == gridParams), It.IsAny<Expression<Func<Material, object>>[]>()), Times.Once);

            _mocker.GetMock<IPersonServiceClient>()
                .Verify(c => c.GetSuppliersAsync(It.Is<IEnumerable<long>>(seq => seq.ToHashSet().SetEquals(new HashSet<long> { supplierIdA, supplierIdB }))), Times.Once);

            _mocker.GetMock<IMapper>().Verify(m => m.Map<Material, MaterialResponseDTO>(It.IsAny<Material>()), Times.Once);
        }

        [Xunit.Fact]
        public async Task GetAsync_WhenNotHaveData_ThrowsInfoException()
        {
            var gridParams = _fixture.CreateGridParamsWithWithoutFilter();
            var repoResponse = (new List<Material>(), new ResponsePagination(1, 10, 0));

            _mocker.GetMock<IMaterialRepository<Material>>()
                .Setup(r => r.GetAsync(It.IsAny<GridParams>(), It.IsAny<Expression<Func<Material, object>>[]>()))
                .ReturnsAsync(repoResponse);

            Func<Task> act = async () => await _service.GetAsync(gridParams);

            var ex = await act.Should().ThrowAsync<InfoException>();
            ex.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.NotFound);

            _mocker.GetMock<IPersonServiceClient>().Verify(c => c.GetSuppliersAsync(It.IsAny<IEnumerable<long>>()), Times.Never);
            _mocker.GetMock<IMapper>().Verify(m => m.Map<Material, MaterialResponseDTO>(It.IsAny<Material>()), Times.Never);
        }

        [Xunit.Fact]
        public async Task GetAsync_WhenRepositoryThrowsDatabaseException_Propagates()
        {
            var gridParams = _fixture.CreateGridParamsWithWithoutFilter();
            var repoEx = new DatabaseException("db error");

            _mocker.GetMock<IMaterialRepository<Material>>()
                .Setup(r => r.GetAsync(It.IsAny<GridParams>(), It.IsAny<Expression<Func<Material, object>>[]>()))
                .ThrowsAsync(repoEx);

            Func<Task> act = async () => await _service.GetAsync(gridParams);

            var ex = await act.Should().ThrowAsync<DatabaseException>();
            ex.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.DatabaseError);

            _mocker.GetMock<IPersonServiceClient>().Verify(c => c.GetSuppliersAsync(It.IsAny<IEnumerable<long>>()), Times.Never);
            _mocker.GetMock<IMapper>().Verify(m => m.Map<Material, MaterialResponseDTO>(It.IsAny<Material>()), Times.Never);
        }

        [Xunit.Fact]
        public async Task GetAsync_WhenPersonClientThrowsDatabaseException_Propagates()
        {
            var faker = _fixture.Faker;
            var gridParams = _fixture.CreateGridParamsWithWithoutFilter();

            var materialId = faker.Random.Long(10, 9999);
            var supplierId = faker.Random.Long(100, 999);

            var material = new Material
            {
                Id = materialId,
                Name = faker.Commerce.ProductName(),
                Suppliers = new List<MaterialSupplier> { new MaterialSupplier { MaterialId = materialId, SupplierId = supplierId } }
            };

            var repoResponse = (new List<Material> { material }, new ResponsePagination(1, 10, 1));
            var clientEx = new DatabaseException("person client db");

            _mocker.GetMock<IMaterialRepository<Material>>()
                .Setup(r => r.GetAsync(It.IsAny<GridParams>(), It.IsAny<Expression<Func<Material, object>>[]>()))
                .ReturnsAsync(repoResponse);

            _mocker.GetMock<IMapper>()
                .Setup(m => m.Map<Material, MaterialResponseDTO>(It.IsAny<Material>()))
                .Returns(new MaterialResponseDTO { Id = material.Id, Name = material.Name, Suppliers = new List<PersonResponseDTO>() });

            _mocker.GetMock<IPersonServiceClient>()
                .Setup(c => c.GetSuppliersAsync(It.IsAny<IEnumerable<long>>()))
                .ThrowsAsync(clientEx);

            Func<Task> act = async () => await _service.GetAsync(gridParams);

            var ex = await act.Should().ThrowAsync<DatabaseException>();
            ex.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.DatabaseError);

            _mocker.GetMock<IPersonServiceClient>().Verify(c => c.GetSuppliersAsync(It.Is<IEnumerable<long>>(seq => seq.ToHashSet().SetEquals(new HashSet<long> { supplierId }))), Times.Once);
            _mocker.GetMock<IMapper>().Verify(m => m.Map<Material, MaterialResponseDTO>(It.IsAny<Material>()), Times.Never);
        }

        [Xunit.Fact]
        public async Task GetAsync_WhenPersonClientReturnsNullData_ThrowsUnexpectedException()
        {
            var faker = _fixture.Faker;
            var gridParams = _fixture.CreateGridParamsWithWithoutFilter();

            var materialId = faker.Random.Long(10, 9999);
            var supplierId = faker.Random.Long(100, 999);
            var getSuppliersResponse = (Response<IEnumerable<PersonResponseDTO>>)null;

            var material = new Material
            {
                Id = materialId,
                Name = faker.Commerce.ProductName(),
                Suppliers = new List<MaterialSupplier> { new MaterialSupplier { MaterialId = materialId, SupplierId = supplierId } }
            };

            var repoResponse = (new List<Material> { material }, new ResponsePagination(1, 10, 1));

            _mocker.GetMock<IMaterialRepository<Material>>()
                .Setup(r => r.GetAsync(It.IsAny<GridParams>(), It.IsAny<Expression<Func<Material, object>>[]>()))
                .ReturnsAsync(repoResponse);

            _mocker.GetMock<IMapper>()
                .Setup(m => m.Map<Material, MaterialResponseDTO>(It.IsAny<Material>()))
                .Returns(new MaterialResponseDTO { Id = material.Id, Name = material.Name, Suppliers = new List<PersonResponseDTO>() });

            _mocker.GetMock<IPersonServiceClient>()
                .Setup(c => c.GetSuppliersAsync(It.IsAny<IEnumerable<long>>()))
                .ReturnsAsync(getSuppliersResponse);

            Func<Task> act = async () => await _service.GetAsync(gridParams);

            var ex = await act.Should().ThrowAsync<UnexpectedException>();
            ex.Which.Message.Should().NotBeNullOrEmpty();

            _mocker.GetMock<IPersonServiceClient>().Verify(c => c.GetSuppliersAsync(It.Is<IEnumerable<long>>(seq => seq.ToHashSet().SetEquals(new HashSet<long> { supplierId }))), Times.Once);
        }

        [Xunit.Fact]
        public async Task GetAsync_WhenPersonClientThrowsBusinessException_Propagates()
        {
            var faker = _fixture.Faker;
            var gridParams = _fixture.CreateGridParamsWithWithoutFilter();

            var materialId = faker.Random.Long(10, 9999);
            var supplierId = faker.Random.Long(100, 999);

            var material = new Material
            {
                Id = materialId,
                Name = faker.Commerce.ProductName(),
                Suppliers = new List<MaterialSupplier> { new MaterialSupplier { MaterialId = materialId, SupplierId = supplierId } }
            };

            var repoResponse = (new List<Material> { material }, new ResponsePagination(1, 10, 1));
            var clientEx = new BusinessException("person client db", ErrorCodeEnum.InternalError);

            _mocker.GetMock<IMaterialRepository<Material>>()
                .Setup(r => r.GetAsync(It.IsAny<GridParams>(), It.IsAny<Expression<Func<Material, object>>[]>()))
                .ReturnsAsync(repoResponse);

            _mocker.GetMock<IMapper>()
                .Setup(m => m.Map<Material, MaterialResponseDTO>(It.IsAny<Material>()))
                .Returns(new MaterialResponseDTO { Id = material.Id, Name = material.Name, Suppliers = new List<PersonResponseDTO>() });

            _mocker.GetMock<IPersonServiceClient>()
                .Setup(c => c.GetSuppliersAsync(It.IsAny<IEnumerable<long>>()))
                .ThrowsAsync(clientEx);

            Func<Task> act = async () => await _service.GetAsync(gridParams);

            var ex = await act.Should().ThrowAsync<BusinessException>();
            ex.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.InternalError);

            _mocker.GetMock<IPersonServiceClient>().Verify(c => c.GetSuppliersAsync(It.Is<IEnumerable<long>>(seq => seq.ToHashSet().SetEquals(new HashSet<long> { supplierId }))), Times.Once);
            _mocker.GetMock<IMapper>().Verify(m => m.Map<Material, MaterialResponseDTO>(It.IsAny<Material>()), Times.Never);
        }

        #endregion

        #region InsertAsync

        [Xunit.Fact]
        public async Task InsertAsync_WhenValid_ReturnsSuccess()
        {
            var mappedEntity = new Material();
            var mappedResponse = new MaterialResponseDTO();
            var validationResult = new ValidationResult();
            var repositoryResponse = new Material();
            var request = new MaterialInsertDTO();

            _mocker.GetMock<IMapper>().Setup(m => m.Map<MaterialInsertDTO, Material>(It.IsAny<MaterialInsertDTO>())).Returns(mappedEntity);
            _mocker.GetMock<IMaterialValidator>().Setup(v => v.ValidateBeforeInsert(It.IsAny<Material>())).Returns(validationResult);
            _mocker.GetMock<IMaterialRepository<Material>>().Setup(r => r.InsertAsync(It.IsAny<Material>())).ReturnsAsync(repositoryResponse);
            _mocker.GetMock<IMapper>().Setup(m => m.Map<Material, MaterialResponseDTO>(It.IsAny<Material>())).Returns(mappedResponse);

            var response = await _service.InsertAsync(request);

            response.Success.Should().BeTrue();
            response.Data.Should().BeSameAs(mappedResponse);

            _mocker.GetMock<IMapper>().Verify(m => m.Map<MaterialInsertDTO, Material>(It.IsAny<MaterialInsertDTO>()), Times.Once);
            _mocker.GetMock<IMaterialValidator>().Verify(v => v.ValidateBeforeInsert(It.IsAny<Material>()), Times.Once);
            _mocker.GetMock<IMaterialRepository<Material>>().Verify(r => r.InsertAsync(It.IsAny<Material>()), Times.Once);
        }

        [Xunit.Fact]
        public async Task InsertAsync_WhenInvalid_ThrowsValidationException()
        {
            var mappedEntity = new Material();
            var validationResult = new ValidationResult { Errors = new List<ValidationFailure> { new ValidationFailure("Prop", "err") } };
            var request = new MaterialInsertDTO();

            _mocker.GetMock<IMapper>().Setup(m => m.Map<MaterialInsertDTO, Material>(It.IsAny<MaterialInsertDTO>())).Returns(mappedEntity);
            _mocker.GetMock<IMaterialValidator>().Setup(v => v.ValidateBeforeInsert(It.IsAny<Material>())).Returns(validationResult);

            Func<Task> act = async () => await _service.InsertAsync(request);

            var ex = await act.Should().ThrowAsync<ValidationException>();
            ex.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.ValidationFailed);

            _mocker.GetMock<IMaterialRepository<Material>>().Verify(r => r.InsertAsync(It.IsAny<Material>()), Times.Never);
        }

        [Xunit.Fact]
        public async Task InsertAsync_WhenRepositoryThrowsDatabaseException_Propagates()
        {
            var mappedEntity = new Material();
            var repoEx = new DatabaseException("db error");
            var validationResult = new ValidationResult();
            var request = new MaterialInsertDTO();

            _mocker.GetMock<IMapper>().Setup(m => m.Map<MaterialInsertDTO, Material>(It.IsAny<MaterialInsertDTO>())).Returns(mappedEntity);
            _mocker.GetMock<IMaterialValidator>().Setup(v => v.ValidateBeforeInsert(It.IsAny<Material>())).Returns(validationResult);
            _mocker.GetMock<IMaterialRepository<Material>>().Setup(r => r.InsertAsync(It.IsAny<Material>())).ThrowsAsync(repoEx);

            Func<Task> act = async () => await _service.InsertAsync(request);

            var ex = await act.Should().ThrowAsync<DatabaseException>();
            ex.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.DatabaseError);

            _mocker.GetMock<IMaterialRepository<Material>>().Verify(r => r.InsertAsync(It.IsAny<Material>()), Times.Once);
        }

        #endregion

        #region UpdateAsync

        [Xunit.Theory]
        [InlineData(2)]
        public async Task UpdateAsync_WhenValid_ReturnsSuccess(long id)
        {
            var mappedEntity = new Material();
            var mappedResponse = new MaterialResponseDTO();
            var validationResult = new ValidationResult();
            var repoResponse = new Material();
            var request = new MaterialUpdateDTO();

            _mocker.GetMock<IMapper>().Setup(m => m.Map<MaterialUpdateDTO, Material>(It.IsAny<MaterialUpdateDTO>())).Returns(mappedEntity);
            _mocker.GetMock<IMaterialValidator>().Setup(v => v.ValidateBeforeUpdate(It.IsAny<Material>())).Returns(validationResult);
            _mocker.GetMock<IMaterialRepository<Material>>().Setup(r => r.UpdateAsync(It.IsAny<long>(), It.IsAny<Material>())).ReturnsAsync(repoResponse);
            _mocker.GetMock<IMapper>().Setup(m => m.Map<Material, MaterialResponseDTO>(It.IsAny<Material>())).Returns(mappedResponse);

            var response = await _service.UpdateAsync(id, request);

            response.Success.Should().BeTrue();
            response.Data.Should().BeSameAs(mappedResponse);

            _mocker.GetMock<IMapper>().Verify(m => m.Map<MaterialUpdateDTO, Material>(It.IsAny<MaterialUpdateDTO>()), Times.Once);
            _mocker.GetMock<IMaterialValidator>().Verify(v => v.ValidateBeforeUpdate(It.IsAny<Material>()), Times.Once);
            _mocker.GetMock<IMaterialRepository<Material>>().Verify(r => r.UpdateAsync(It.Is<long>(p => p == id), It.IsAny<Material>()), Times.Once);
        }

        [Xunit.Theory]
        [InlineData(2)]
        public async Task UpdateAsync_WhenInvalid_ThrowsValidationException(long id)
        {
            var mappedEntity = new Material();
            var validationResult = new ValidationResult { Errors = new List<ValidationFailure> { new ValidationFailure("Prop", "err") } };
            var request = new MaterialUpdateDTO();

            _mocker.GetMock<IMapper>().Setup(m => m.Map<MaterialUpdateDTO, Material>(It.IsAny<MaterialUpdateDTO>())).Returns(mappedEntity);
            _mocker.GetMock<IMaterialValidator>().Setup(v => v.ValidateBeforeUpdate(It.IsAny<Material>())).Returns(validationResult);

            Func<Task> act = async () => await _service.UpdateAsync(id, request);

            var ex = await act.Should().ThrowAsync<ValidationException>();
            ex.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.ValidationFailed);

            _mocker.GetMock<IMaterialRepository<Material>>().Verify(r => r.UpdateAsync(It.IsAny<long>(), It.IsAny<Material>()), Times.Never);
        }

        [Xunit.Theory]
        [InlineData(2)]
        public async Task UpdateAsync_WhenRepositoryThrowsDatabaseException_Propagates(long id)
        {
            var mappedEntity = new Material();
            var repoEx = new DatabaseException("db error");
            var validationResult = new ValidationResult();
            var request = new MaterialUpdateDTO();

            _mocker.GetMock<IMapper>().Setup(m => m.Map<MaterialUpdateDTO, Material>(It.IsAny<MaterialUpdateDTO>())).Returns(mappedEntity);
            _mocker.GetMock<IMaterialValidator>().Setup(v => v.ValidateBeforeUpdate(It.IsAny<Material>())).Returns(validationResult);
            _mocker.GetMock<IMaterialRepository<Material>>().Setup(r => r.UpdateAsync(It.IsAny<long>(), It.IsAny<Material>())).ThrowsAsync(repoEx);

            Func<Task> act = async () => await _service.UpdateAsync(id, request);

            var ex = await act.Should().ThrowAsync<DatabaseException>();
            ex.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.DatabaseError);

            _mocker.GetMock<IMaterialRepository<Material>>().Verify(r => r.UpdateAsync(It.Is<long>(p => p == id), It.IsAny<Material>()), Times.Once);
        }

        #endregion

        #region AddSuppliersAsync

        [Xunit.Fact]
        public async Task AddSuppliersAsync_WhenMaterialNotFound_ThrowsInfoException()
        {
            var materialId = 5;
            var dto = new MaterialAddSuppliersDTO { SupplierIds = new List<long> { 1, 2 } };

            _mocker.GetMock<IMaterialRepository<Material>>()
                .Setup(r => r.GetByIdAsync(It.Is<long>(p => p == materialId), It.IsAny<Expression<Func<Material, object>>[]>()))
                .ReturnsAsync((Material)null);

            Func<Task> act = async () => await _service.AddSuppliersAsync(materialId, dto);

            var ex = await act.Should().ThrowAsync<InfoException>();
            ex.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.NotFound);

            _mocker.GetMock<IMaterialRepository<Material>>().Verify(r => r.AddSuppliersAsync(It.IsAny<long>(), It.IsAny<IEnumerable<MaterialSupplier>>()), Times.Never);
        }

        [Xunit.Fact]
        public async Task AddSuppliersAsync_WhenExceedsLimit_ThrowsInfoException()
        {
            var materialId = 6;
            var existing = Enumerable.Range(1, 4).Select(i => new MaterialSupplier { MaterialId = materialId, SupplierId = i }).ToList();
            var dto = new MaterialAddSuppliersDTO { SupplierIds = new List<long> { 10, 11 } };

            var material = new Material { Id = materialId, Suppliers = existing };

            _mocker.GetMock<IMaterialRepository<Material>>()
                .Setup(r => r.GetByIdAsync(It.Is<long>(p => p == materialId), It.IsAny<Expression<Func<Material, object>>[]>()))
                .ReturnsAsync(material);

            Func<Task> act = async () => await _service.AddSuppliersAsync(materialId, dto);

            var ex = await act.Should().ThrowAsync<InfoException>();
            ex.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.NotFound);

            _mocker.GetMock<IMaterialRepository<Material>>().Verify(r => r.AddSuppliersAsync(It.IsAny<long>(), It.IsAny<IEnumerable<MaterialSupplier>>()), Times.Never);
        }

        [Xunit.Fact]
        public async Task AddSuppliersAsync_WhenValidatorInvalid_ThrowsValidationException()
        {
            var materialId = 7;
            var material = new Material { Id = materialId, Suppliers = new List<MaterialSupplier>() };
            var dto = new MaterialAddSuppliersDTO { SupplierIds = new List<long> { 1, 2 } };

            var validationResult = new ValidationResult { Errors = new List<ValidationFailure> { new ValidationFailure("supplier", "invalid") } };

            _mocker.GetMock<IMaterialRepository<Material>>()
                .Setup(r => r.GetByIdAsync(It.IsAny<long>(), It.IsAny<Expression<Func<Material, object>>[]>()))
                .ReturnsAsync(material);

            _mocker.GetMock<IMaterialValidator>()
                .Setup(v => v.ValidateSuppliersAsync(It.IsAny<IEnumerable<long>>()))
                .Returns(validationResult);

            Func<Task> act = async () => await _service.AddSuppliersAsync(materialId, dto);

            var ex = await act.Should().ThrowAsync<ValidationException>();
            ex.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.ValidationFailed);

            _mocker.GetMock<IMaterialRepository<Material>>().Verify(r => r.AddSuppliersAsync(It.IsAny<long>(), It.IsAny<IEnumerable<MaterialSupplier>>()), Times.Never);
        }

        [Xunit.Fact]
        public async Task AddSuppliersAsync_WhenValid_AddsSuppliers()
        {
            var materialId = 8;
            var material = new Material { Id = materialId, Suppliers = new List<MaterialSupplier>() };
            var dto = new MaterialAddSuppliersDTO { SupplierIds = new List<long> { 21, 22 } };
            var validationResult = new ValidationResult();

            _mocker.GetMock<IMaterialRepository<Material>>()
                .Setup(r => r.GetByIdAsync(It.IsAny<long>(), It.IsAny<Expression<Func<Material, object>>[]>()))
                .ReturnsAsync(material);

            _mocker.GetMock<IMaterialValidator>()
                .Setup(v => v.ValidateSuppliersAsync(It.IsAny<IEnumerable<long>>()))
                .Returns(validationResult);

            _mocker.GetMock<IMaterialRepository<Material>>()
                .Setup(r => r.AddSuppliersAsync(It.IsAny<long>(), It.IsAny<IEnumerable<MaterialSupplier>>()))
                .ReturnsAsync(material);

            var response = await _service.AddSuppliersAsync(materialId, dto);

            response.Success.Should().BeTrue();

            _mocker.GetMock<IMaterialRepository<Material>>().Verify(r => r.AddSuppliersAsync(It.Is<long>(p => p == materialId),
                It.Is<IEnumerable<MaterialSupplier>>(list => list.Count() == dto.SupplierIds.Count() && list.All(x => x.MaterialId == materialId))), Times.Once);
        }

        #endregion

        #region RemoveSuppliersAsync

        [Xunit.Fact]
        public async Task RemoveSuppliersAsync_WhenMaterialNotFound_ThrowsInfoException()
        {
            var materialId = 9;
            var material = (Material)null;
            var dto = new MaterialRemoveSuppliersDTO { SupplierIds = new List<long> { 1 } };

            _mocker.GetMock<IMaterialRepository<Material>>()
                .Setup(r => r.GetByIdAsync(It.Is<long>(p => p == materialId), It.IsAny<Expression<Func<Material, object>>[]>()))
                .ReturnsAsync(material);

            Func<Task> act = async () => await _service.RemoveSuppliersAsync(materialId, dto);

            var ex = await act.Should().ThrowAsync<InfoException>();
            ex.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.NotFound);

            _mocker.GetMock<IMaterialRepository<Material>>().Verify(r => r.RemoveSuppliersAsync(It.IsAny<long>(), It.IsAny<IEnumerable<MaterialSupplier>>()), Times.Never);
        }

        [Xunit.Fact]
        public async Task RemoveSuppliersAsync_WhenHaveSuppliers_RemovesThem()
        {
            var materialId = 11;
            var material = new Material { Id = materialId };
            var dto = new MaterialRemoveSuppliersDTO { SupplierIds = new List<long> { 101, 102 } };

            _mocker.GetMock<IMaterialRepository<Material>>()
                .Setup(r => r.GetByIdAsync(It.Is<long>(p => p == materialId), It.IsAny<Expression<Func<Material, object>>[]>()))
                .ReturnsAsync(material);

            var suppliersToRemove = new List<MaterialSupplier>
            {
                new MaterialSupplier { MaterialId = materialId, SupplierId = 101 },
                new MaterialSupplier { MaterialId = materialId, SupplierId = 102 }
            };

            _mocker.GetMock<IMaterialSupplierService>()
                .Setup(s => s.GetAsync(It.IsAny<GridParams>()))
                .ReturnsAsync(suppliersToRemove);

            _mocker.GetMock<IMaterialRepository<Material>>()
                .Setup(r => r.RemoveSuppliersAsync(It.IsAny<long>(), It.IsAny<IEnumerable<MaterialSupplier>>()))
                .ReturnsAsync(material);

            var response = await _service.RemoveSuppliersAsync(materialId, dto);

            response.Success.Should().BeTrue();

            _mocker.GetMock<IMaterialSupplierService>()
                .Verify(s => s.GetAsync(It.Is<GridParams>(g => g.Filters != null && g.Filters.Any(f => f.Field == "materialId" && f.Value.ToString() == materialId.ToString()))), Times.Once);

            _mocker.GetMock<IMaterialRepository<Material>>()
                .Verify(r => r.RemoveSuppliersAsync(It.Is<long>(p => p == materialId),
                    It.Is<IEnumerable<MaterialSupplier>>(list => list.Count() == suppliersToRemove.Count)), Times.Once);
        }

        #endregion
    }
}
