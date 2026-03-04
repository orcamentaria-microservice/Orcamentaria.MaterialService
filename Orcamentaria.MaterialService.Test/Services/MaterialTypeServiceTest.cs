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
using Orcamentaria.Lib.Domain.Validators;
using Orcamentaria.MaterialService.Domain.DTOs.MaterialType;
using Orcamentaria.MaterialService.Domain.Models;
using Orcamentaria.MaterialService.Domain.Repositories;
using Orcamentaria.MaterialService.Test.Fixtures;
using Orcamentaria.MaterialService.Application.Services;
using Xunit;

namespace Orcamentaria.MaterialService.Test.Services
{
    [Collection(nameof(MaterialTypeCollection))]
    public class MaterialTypeServiceTest
    {
        private readonly MaterialTypeFixture _fixture;
        private readonly AutoMocker _mocker;
        private readonly MaterialTypeService _service;

        public MaterialTypeServiceTest(MaterialTypeFixture fixture)
        {
            _fixture = fixture;
            _mocker = new AutoMocker();
            _service = _mocker.CreateInstance<MaterialTypeService>(true);
        }

        #region GetByIdAsync

        [Xunit.Theory]
        [InlineData(1)]
        [InlineData(5)]
        public async Task GetByIdAsync_WhenHaveData_ReturnsData(long id)
        {
            var entity = _fixture.CreateEntity(id);

            _mocker.GetMock<IMaterialTypeRepository<MaterialType>>()
                .Setup(r => r.GetByIdAsync(It.Is<long>(p => p == id)))
                .ReturnsAsync(entity);

            var result = await _service.GetByIdAsync(id);

            result.Should().NotBeNull();
            result.Should().BeSameAs(entity);

            _mocker.GetMock<IMaterialTypeRepository<MaterialType>>()
                .Verify(r => r.GetByIdAsync(It.Is<long>(p => p == id)), Times.Once);
        }

        [Xunit.Theory]
        [InlineData(0)]
        public async Task GetByIdAsync_WhenNotHaveData_ReturnsNull(long id)
        {
            _mocker.GetMock<IMaterialTypeRepository<MaterialType>>()
                .Setup(r => r.GetByIdAsync(It.Is<long>(p => p == id)))
                .ReturnsAsync((MaterialType?)null);

            var result = await _service.GetByIdAsync(id);

            result.Should().BeNull();

            _mocker.GetMock<IMaterialTypeRepository<MaterialType>>()
                .Verify(r => r.GetByIdAsync(It.Is<long>(p => p == id)), Times.Once);
        }

        [Xunit.Theory]
        [InlineData(99)]
        public async Task GetByIdAsync_WhenRepositoryThrowsDatabaseException_Propagates(long id)
        {
            var repoEx = new DatabaseException("db error");

            _mocker.GetMock<IMaterialTypeRepository<MaterialType>>()
                .Setup(r => r.GetByIdAsync(It.IsAny<long>()))
                .ThrowsAsync(repoEx);

            Func<Task> act = async () => await _service.GetByIdAsync(id);

            var ex = await act.Should().ThrowAsync<DatabaseException>();
            ex.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.DatabaseError);

            _mocker.GetMock<IMaterialTypeRepository<MaterialType>>()
                .Verify(r => r.GetByIdAsync(It.IsAny<long>()), Times.Once);
        }

        #endregion

        #region GetAsync

        [Xunit.Fact]
        public async Task GetAsync_WhenHaveData_ReturnsData()
        {
            var faker = _fixture.Faker;
            var gridParams = _fixture.CreateGridParamsWithWithoutFilter();

            var list = new List<MaterialType>
            {
                _fixture.CreateEntity(faker.Random.Long(1,9999)),
                _fixture.CreateEntity(faker.Random.Long(10000,19999))
            };

            var repoResponse = (list.AsEnumerable(), new ResponsePagination(1, 10, list.Count));

            var mappedDto = new MaterialTypeResponseDTO();

            _mocker.GetMock<IMaterialTypeRepository<MaterialType>>()
                .Setup(r => r.GetAsync(It.Is<GridParams>(g => g == gridParams)))
                .ReturnsAsync(repoResponse);

            _mocker.GetMock<IMapper>()
                .Setup(m => m.Map<MaterialType, MaterialTypeResponseDTO>(It.IsAny<MaterialType>()))
                .Returns(mappedDto);

            var response = await _service.GetAsync(gridParams);

            response.Should().NotBeNull();
            response.Success.Should().BeTrue();
            response.Data.Should().HaveCount(list.Count);
            response.Data.All(d => d == mappedDto).Should().BeTrue();

            _mocker.GetMock<IMaterialTypeRepository<MaterialType>>()
                .Verify(r => r.GetAsync(It.Is<GridParams>(g => g == gridParams)), Times.Once);
        }

        [Xunit.Fact]
        public async Task GetAsync_WhenNotHaveData_ThrowsInfoException()
        {
            var gridParams = _fixture.CreateGridParamsWithWithoutFilter();
            var repoResponse = (Enumerable.Empty<MaterialType>(), new ResponsePagination(1, 10, 0));

            _mocker.GetMock<IMaterialTypeRepository<MaterialType>>()
                .Setup(r => r.GetAsync(It.IsAny<GridParams>()))
                .ReturnsAsync(repoResponse);

            Func<Task> act = async () => await _service.GetAsync(gridParams);

            var ex = await act.Should().ThrowAsync<InfoException>();
            ex.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.NotFound);

            _mocker.GetMock<IMapper>().Verify(m => m.Map<MaterialType, MaterialTypeResponseDTO>(It.IsAny<MaterialType>()), Times.Never);
        }

        [Xunit.Fact]
        public async Task GetAsync_WhenRepositoryThrowsDatabaseException_Propagates()
        {
            var gridParams = _fixture.CreateGridParamsWithWithoutFilter();
            var repoEx = new DatabaseException("db error");

            _mocker.GetMock<IMaterialTypeRepository<MaterialType>>()
                .Setup(r => r.GetAsync(It.IsAny<GridParams>()))
                .ThrowsAsync(repoEx);

            Func<Task> act = async () => await _service.GetAsync(gridParams);

            var ex = await act.Should().ThrowAsync<DatabaseException>();
            ex.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.DatabaseError);

            _mocker.GetMock<IMapper>().Verify(m => m.Map<MaterialType, MaterialTypeResponseDTO>(It.IsAny<MaterialType>()), Times.Never);
        }

        #endregion

        #region InsertAsync

        [Xunit.Fact]
        public async Task InsertAsync_WhenValid_UppercasesNameAndReturnsMapped()
        {
            var faker = _fixture.Faker;
            var dto = new MaterialTypeInsertDTO { Name = faker.Commerce.Product() };
            var mappedEntity = new MaterialType { Name = dto.Name };
            var savedEntity = new MaterialType { Id = faker.Random.Long(1, 9999), Name = dto.Name.ToUpper() };
            var mappedResponse = new MaterialTypeResponseDTO();

            _mocker.GetMock<IMapper>()
                .Setup(m => m.Map<MaterialTypeInsertDTO, MaterialType>(It.IsAny<MaterialTypeInsertDTO>()))
                .Returns(mappedEntity);

            _mocker.GetMock<IValidatorEntity<MaterialType>>()
                .Setup(v => v.ValidateBeforeInsert(It.IsAny<MaterialType>()))
                .Returns(new ValidationResult());

            _mocker.GetMock<IMaterialTypeRepository<MaterialType>>()
                .Setup(r => r.InsertAsync(It.IsAny<MaterialType>()))
                .ReturnsAsync(savedEntity);

            _mocker.GetMock<IMapper>()
                .Setup(m => m.Map<MaterialType, MaterialTypeResponseDTO>(It.IsAny<MaterialType>()))
                .Returns(mappedResponse);

            var response = await _service.InsertAsync(dto);

            response.Success.Should().BeTrue();
            response.Data.Should().BeSameAs(mappedResponse);

            _mocker.GetMock<IMapper>().Verify(m => m.Map<MaterialTypeInsertDTO, MaterialType>(It.IsAny<MaterialTypeInsertDTO>()), Times.Once);
            _mocker.GetMock<IValidatorEntity<MaterialType>>().Verify(v => v.ValidateBeforeInsert(It.Is<MaterialType>(mt => mt.Name == dto.Name.ToUpper())), Times.Once);
            _mocker.GetMock<IMaterialTypeRepository<MaterialType>>().Verify(r => r.InsertAsync(It.IsAny<MaterialType>()), Times.Once);
            _mocker.GetMock<IMapper>().Verify(m => m.Map<MaterialType, MaterialTypeResponseDTO>(It.IsAny<MaterialType>()), Times.Once);
        }

        [Xunit.Fact]
        public async Task InsertAsync_WhenInvalid_ThrowsValidationException()
        {
            var dto = new MaterialTypeInsertDTO { Name = "x" };
            var entity = new MaterialType { Name = dto.Name };
            var validation = new ValidationResult { Errors = new List<ValidationFailure> { new ValidationFailure("Name", "err") } };

            _mocker.GetMock<IMapper>().Setup(m => m.Map<MaterialTypeInsertDTO, MaterialType>(It.IsAny<MaterialTypeInsertDTO>())).Returns(entity);
            _mocker.GetMock<IValidatorEntity<MaterialType>>().Setup(v => v.ValidateBeforeInsert(It.IsAny<MaterialType>())).Returns(validation);

            Func<Task> act = async () => await _service.InsertAsync(dto);

            var ex = await act.Should().ThrowAsync<ValidationException>();
            ex.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.ValidationFailed);

            _mocker.GetMock<IMaterialTypeRepository<MaterialType>>().Verify(r => r.InsertAsync(It.IsAny<MaterialType>()), Times.Never);
            _mocker.GetMock<IMapper>().Verify(m => m.Map<MaterialType, MaterialTypeResponseDTO>(It.IsAny<MaterialType>()), Times.Never);
        }

        [Xunit.Fact]
        public async Task InsertAsync_WhenRepositoryThrowsDatabaseException_Propagates()
        {
            var dto = new MaterialTypeInsertDTO { Name = "valid" };
            var entity = new MaterialType { Name = dto.Name };
            var validation = new ValidationResult();
            var repoEx = new DatabaseException("db");

            _mocker.GetMock<IMapper>().Setup(m => m.Map<MaterialTypeInsertDTO, MaterialType>(It.IsAny<MaterialTypeInsertDTO>())).Returns(entity);
            _mocker.GetMock<IValidatorEntity<MaterialType>>().Setup(v => v.ValidateBeforeInsert(It.IsAny<MaterialType>())).Returns(validation);
            _mocker.GetMock<IMaterialTypeRepository<MaterialType>>().Setup(r => r.InsertAsync(It.IsAny<MaterialType>())).ThrowsAsync(repoEx);

            Func<Task> act = async () => await _service.InsertAsync(dto);

            var ex = await act.Should().ThrowAsync<DatabaseException>();
            ex.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.DatabaseError);

            _mocker.GetMock<IMaterialTypeRepository<MaterialType>>().Verify(r => r.InsertAsync(It.IsAny<MaterialType>()), Times.Once);
        }

        #endregion

        #region UpdateAsync

        [Xunit.Theory]
        [InlineData(1)]
        [InlineData(3)]
        public async Task UpdateAsync_WhenValid_UppercasesNameAndReturnsMapped(long id)
        {
            var faker = _fixture.Faker;
            var dto = new MaterialTypeUpdateDTO { Name = faker.Commerce.Product() };
            var mappedEntity = new MaterialType { Name = dto.Name };
            var savedEntity = new MaterialType { Id = id, Name = dto.Name.ToUpper() };
            var mappedResponse = new MaterialTypeResponseDTO();

            _mocker.GetMock<IMapper>().Setup(m => m.Map<MaterialTypeUpdateDTO, MaterialType>(It.IsAny<MaterialTypeUpdateDTO>())).Returns(mappedEntity);
            _mocker.GetMock<IValidatorEntity<MaterialType>>().Setup(v => v.ValidateBeforeUpdate(It.IsAny<MaterialType>())).Returns(new ValidationResult());
            _mocker.GetMock<IMaterialTypeRepository<MaterialType>>().Setup(r => r.UpdateAsync(It.IsAny<long>(), It.IsAny<MaterialType>())).ReturnsAsync(savedEntity);
            _mocker.GetMock<IMapper>().Setup(m => m.Map<MaterialType, MaterialTypeResponseDTO>(It.IsAny<MaterialType>())).Returns(mappedResponse);

            var response = await _service.UpdateAsync(id, dto);

            response.Success.Should().BeTrue();
            response.Data.Should().BeSameAs(mappedResponse);

            _mocker.GetMock<IMapper>().Verify(m => m.Map<MaterialTypeUpdateDTO, MaterialType>(It.IsAny<MaterialTypeUpdateDTO>()), Times.Once);
            _mocker.GetMock<IValidatorEntity<MaterialType>>().Verify(v => v.ValidateBeforeUpdate(It.Is<MaterialType>(mt => mt.Id == id && mt.Name == dto.Name.ToUpper())), Times.Once);
            _mocker.GetMock<IMaterialTypeRepository<MaterialType>>().Verify(r => r.UpdateAsync(It.Is<long>(p => p == id), It.IsAny<MaterialType>()), Times.Once);
            _mocker.GetMock<IMapper>().Verify(m => m.Map<MaterialType, MaterialTypeResponseDTO>(It.IsAny<MaterialType>()), Times.Once);
        }

        [Xunit.Theory]
        [InlineData(1)]
        public async Task UpdateAsync_WhenInvalid_ThrowsValidationException(long id)
        {
            var dto = new MaterialTypeUpdateDTO { Name = "x" };
            var entity = new MaterialType { Name = dto.Name };
            var validation = new ValidationResult { Errors = new List<ValidationFailure> { new ValidationFailure("Name", "err") } };

            _mocker.GetMock<IMapper>().Setup(m => m.Map<MaterialTypeUpdateDTO, MaterialType>(It.IsAny<MaterialTypeUpdateDTO>())).Returns(entity);
            _mocker.GetMock<IValidatorEntity<MaterialType>>().Setup(v => v.ValidateBeforeUpdate(It.IsAny<MaterialType>())).Returns(validation);

            Func<Task> act = async () => await _service.UpdateAsync(id, dto);

            var ex = await act.Should().ThrowAsync<ValidationException>();
            ex.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.ValidationFailed);

            _mocker.GetMock<IMaterialTypeRepository<MaterialType>>().Verify(r => r.UpdateAsync(It.IsAny<long>(), It.IsAny<MaterialType>()), Times.Never);
        }

        [Xunit.Theory]
        [InlineData(2)]
        public async Task UpdateAsync_WhenRepositoryThrowsDatabaseException_Propagates(long id)
        {
            var dto = new MaterialTypeUpdateDTO { Name = "valid" };
            var entity = new MaterialType { Name = dto.Name };
            var validation = new ValidationResult();
            var repoEx = new DatabaseException("db");

            _mocker.GetMock<IMapper>().Setup(m => m.Map<MaterialTypeUpdateDTO, MaterialType>(It.IsAny<MaterialTypeUpdateDTO>())).Returns(entity);
            _mocker.GetMock<IValidatorEntity<MaterialType>>().Setup(v => v.ValidateBeforeUpdate(It.IsAny<MaterialType>())).Returns(validation);
            _mocker.GetMock<IMaterialTypeRepository<MaterialType>>().Setup(r => r.UpdateAsync(It.IsAny<long>(), It.IsAny<MaterialType>())).ThrowsAsync(repoEx);

            Func<Task> act = async () => await _service.UpdateAsync(id, dto);

            var ex = await act.Should().ThrowAsync<DatabaseException>();
            ex.Which.ErrorCode.Should().Be((int)ErrorCodeEnum.DatabaseError);

            _mocker.GetMock<IMaterialTypeRepository<MaterialType>>().Verify(r => r.UpdateAsync(It.Is<long>(p => p == id), It.IsAny<MaterialType>()), Times.Once);
        }

        #endregion
    }
}
