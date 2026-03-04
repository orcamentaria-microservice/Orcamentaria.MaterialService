using FluentAssertions;
using Moq;
using Moq.AutoMock;
using Orcamentaria.Lib.Domain.Enums;
using Orcamentaria.Lib.Domain.Exceptions;
using Orcamentaria.Lib.Domain.Models.Responses;
using Orcamentaria.MaterialService.Application.Validators;
using Orcamentaria.MaterialService.Domain.DTOs.Person;
using Orcamentaria.MaterialService.Domain.Models;
using Orcamentaria.MaterialService.Domain.Repositories;
using Orcamentaria.MaterialService.Domain.ServiceClient;
using Orcamentaria.MaterialService.Domain.Services;
using Orcamentaria.MaterialService.Test.Fixtures;
using Xunit;

namespace Orcamentaria.MaterialService.Test.Validators
{
    [Collection(nameof(MaterialCollection))]
    public class MaterialValidatorTest
    {
        private readonly MaterialFixture _fixture;
        private readonly AutoMocker _mocker;
        private readonly MaterialValidator _validator;

        public MaterialValidatorTest(MaterialFixture fixture)
        {
            _fixture = fixture;
            _mocker = new AutoMocker();
            _validator = _mocker.CreateInstance<MaterialValidator>(true);
        }

        #region ValidateBeforeInsert

        [Fact]
        public void ValidateBeforeInsert_WhenEntityValid_ReturnsIsValid()
        {
            var entity = _fixture.CreateEntity(0);

            _mocker.GetMock<IMaterialTypeService>()
                .Setup(s => s.GetByIdAsync(It.IsAny<long>()))
                .ReturnsAsync(new MaterialType());

            var result = _validator.ValidateBeforeInsert(entity);

            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();

            _mocker.GetMock<IMaterialTypeService>()
                .Verify(s => s.GetByIdAsync(It.IsAny<long>()), Times.Once());
        }

        [Fact]
        public void ValidateBeforeInsert_WhenIdGreaterThanZero_ReturnsInvalidAndErrorMessage()
        {
            var entity = _fixture.CreateEntity(1);

            _mocker.GetMock<IMaterialTypeService>()
                .Setup(s => s.GetByIdAsync(It.IsAny<long>()))
                .ReturnsAsync(new MaterialType());

            var result = _validator.ValidateBeforeInsert(entity);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => x.ErrorMessage == "O Id não deve ser informado.");

            _mocker.GetMock<IMaterialTypeService>()
                .Verify(s => s.GetByIdAsync(It.IsAny<long>()), Times.Once());
        }

        [Fact]
        public void ValidateBeforeInsert_WhenNameIsEmpty_ReturnsInvalidAndErrorMessage()
        {
            var entity = _fixture.CreateEntity(0);
            entity.Name = string.Empty;

            _mocker.GetMock<IMaterialTypeService>()
                .Setup(s => s.GetByIdAsync(It.IsAny<long>()))
                .ReturnsAsync(new MaterialType());

            var result = _validator.ValidateBeforeInsert(entity);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => x.ErrorMessage == "O Name é obrigatório.");

            _mocker.GetMock<IMaterialTypeService>()
                .Verify(s => s.GetByIdAsync(It.IsAny<long>()), Times.Once());
        }

        [Fact]
        public void ValidateBeforeInsert_WhenNameLengthGreaterThan60_ReturnsInvalidAndErrorMessage()
        {
            var entity = _fixture.CreateEntity(0);
            entity.Name = _fixture.Faker.Random.AlphaNumeric(61);

            _mocker.GetMock<IMaterialTypeService>()
                .Setup(s => s.GetByIdAsync(It.IsAny<long>()))
                .ReturnsAsync(new MaterialType());

            var result = _validator.ValidateBeforeInsert(entity);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => x.ErrorMessage == "O tamanho máximo do Name é de 60 caracteres.");

            _mocker.GetMock<IMaterialTypeService>()
                .Verify(s => s.GetByIdAsync(It.IsAny<long>()), Times.Once());
        }

        [Fact]
        public void ValidateBeforeInsert_WhenDescriptionLengthGreaterThan256_ReturnsInvalidAndErrorMessage()
        {
            var entity = _fixture.CreateEntity(0);
            entity.Description = _fixture.Faker.Random.AlphaNumeric(257);

            _mocker.GetMock<IMaterialTypeService>()
                .Setup(s => s.GetByIdAsync(It.IsAny<long>()))
                .ReturnsAsync(new MaterialType());

            var result = _validator.ValidateBeforeInsert(entity);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => x.ErrorMessage == "O tamanho máximo do Description é de 256 caracteres.");

            _mocker.GetMock<IMaterialTypeService>()
                .Verify(s => s.GetByIdAsync(It.IsAny<long>()), Times.Once());
        }

        [Fact]
        public void ValidateBeforeInsert_WhenManufacturerLengthGreaterThan150_ReturnsInvalidAndErrorMessage()
        {
            var entity = _fixture.CreateEntity(0);
            entity.Manufacturer = _fixture.Faker.Random.AlphaNumeric(151);

            _mocker.GetMock<IMaterialTypeService>()
                .Setup(s => s.GetByIdAsync(It.IsAny<long>()))
                .ReturnsAsync(new MaterialType());

            var result = _validator.ValidateBeforeInsert(entity);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => x.ErrorMessage == "O tamanho máximo do Manufacturer é de 150 caracteres.");

            _mocker.GetMock<IMaterialTypeService>()
                .Verify(s => s.GetByIdAsync(It.IsAny<long>()), Times.Once());
        }

        [Fact]
        public void ValidateBeforeInsert_WhenTypeNotFound_ReturnsInvalidAndErrorMessage()
        {
            var entity = _fixture.CreateEntity(0);

            _mocker.GetMock<IMaterialTypeService>()
                .Setup(s => s.GetByIdAsync(It.IsAny<long>()))
                .ReturnsAsync((MaterialType)null);

            var result = _validator.ValidateBeforeInsert(entity);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => x.ErrorMessage == "O tipo informado não existe.");

            _mocker.GetMock<IMaterialTypeService>()
                .Verify(s => s.GetByIdAsync(It.IsAny<long>()), Times.Once());
        }

        #endregion

        #region ValidateBeforeUpdate

        [Fact]
        public void ValidateBeforeUpdate_WhenEntityValid_ReturnsIsValid()
        {
            var entity = _fixture.CreateEntity(1);

            _mocker.GetMock<IMaterialRepository<Material>>()
                .Setup(r => r.GetByIdAsync(It.IsAny<long>()))
                .ReturnsAsync(entity);

            _mocker.GetMock<IMaterialTypeService>()
                .Setup(s => s.GetByIdAsync(It.IsAny<long>()))
                .ReturnsAsync(new MaterialType());

            var result = _validator.ValidateBeforeUpdate(entity);

            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();

            _mocker.GetMock<IMaterialRepository<Material>>()
                .Verify(r => r.GetByIdAsync(It.IsAny<long>()), Times.Once());

            _mocker.GetMock<IMaterialTypeService>()
                .Verify(s => s.GetByIdAsync(It.IsAny<long>()), Times.Once());
        }

        [Fact]
        public void ValidateBeforeUpdate_WhenIdNotInformed_ReturnsInvalidAndErrorMessage()
        {
            var entity = _fixture.CreateEntity(0);

            // repository will return null for id 0
            _mocker.GetMock<IMaterialRepository<Material>>()
                .Setup(r => r.GetByIdAsync(It.IsAny<long>()))
                .ReturnsAsync((Material)null);

            _mocker.GetMock<IMaterialTypeService>()
                .Setup(s => s.GetByIdAsync(It.IsAny<long>()))
                .ReturnsAsync(new MaterialType());

            var result = _validator.ValidateBeforeUpdate(entity);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => x.ErrorMessage == "O Id deve ser informado.");

            _mocker.GetMock<IMaterialRepository<Material>>()
                .Verify(r => r.GetByIdAsync(It.IsAny<long>()), Times.Once());

            _mocker.GetMock<IMaterialTypeService>()
                .Verify(s => s.GetByIdAsync(It.IsAny<long>()), Times.Once());
        }

        [Fact]
        public void ValidateBeforeUpdate_WhenIdNotFound_ReturnsInvalidAndErrorMessage()
        {
            var entity = _fixture.CreateEntity(52);

            _mocker.GetMock<IMaterialRepository<Material>>()
                .Setup(r => r.GetByIdAsync(It.IsAny<long>()))
                .ReturnsAsync((Material)null);

            _mocker.GetMock<IMaterialTypeService>()
                .Setup(s => s.GetByIdAsync(It.IsAny<long>()))
                .ReturnsAsync(new MaterialType());

            var result = _validator.ValidateBeforeUpdate(entity);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => x.ErrorMessage == "Id não encontrado.");

            _mocker.GetMock<IMaterialRepository<Material>>()
                .Verify(r => r.GetByIdAsync(It.IsAny<long>()), Times.Once());

            _mocker.GetMock<IMaterialTypeService>()
                .Verify(s => s.GetByIdAsync(It.IsAny<long>()), Times.Once());
        }

        [Fact]
        public void ValidateBeforeUpdate_WhenNameIsEmpty_ReturnsInvalidAndErrorMessage()
        {
            var entity = _fixture.CreateEntity(1);
            entity.Name = string.Empty;

            _mocker.GetMock<IMaterialRepository<Material>>()
                .Setup(r => r.GetByIdAsync(It.IsAny<long>()))
                .ReturnsAsync(entity);

            _mocker.GetMock<IMaterialTypeService>()
                .Setup(s => s.GetByIdAsync(It.IsAny<long>()))
                .ReturnsAsync(new MaterialType());

            var result = _validator.ValidateBeforeUpdate(entity);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => x.ErrorMessage == "O Name é obrigatório.");

            _mocker.GetMock<IMaterialRepository<Material>>()
                .Verify(r => r.GetByIdAsync(It.IsAny<long>()), Times.Once());

            _mocker.GetMock<IMaterialTypeService>()
                .Verify(s => s.GetByIdAsync(It.IsAny<long>()), Times.Once());
        }

        [Fact]
        public void ValidateBeforeUpdate_WhenTypeNotFound_ReturnsInvalidAndErrorMessage()
        {
            var entity = _fixture.CreateEntity(1);

            _mocker.GetMock<IMaterialRepository<Material>>()
                .Setup(r => r.GetByIdAsync(It.IsAny<long>()))
                .ReturnsAsync(entity);

            _mocker.GetMock<IMaterialTypeService>()
                .Setup(s => s.GetByIdAsync(It.IsAny<long>()))
                .ReturnsAsync((MaterialType)null);

            var result = _validator.ValidateBeforeUpdate(entity);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => x.ErrorMessage == "O tipo informado não existe.");

            _mocker.GetMock<IMaterialRepository<Material>>()
                .Verify(r => r.GetByIdAsync(It.IsAny<long>()), Times.Once());

            _mocker.GetMock<IMaterialTypeService>()
                .Verify(s => s.GetByIdAsync(It.IsAny<long>()), Times.Once());
        }

        #endregion

        #region ValidateSuppliersAsync
        [Fact]
        public void ValidateSuppliersAsync_WhenAllSupplierIdsValid_ReturnsValid()
        {
            var suppliers = new List<long> { 5, 6 };

            var data = new List<PersonResponseDTO>
            {
                new PersonResponseDTO { Id = 5, Name = "X" },
                new PersonResponseDTO { Id = 6, Name = "Y" }
            };

            var response = new Response<IEnumerable<PersonResponseDTO>>(data);

            _mocker.GetMock<IPersonServiceClient>()
                .Setup(s => s.GetSuppliersAsync(It.IsAny<IEnumerable<long>>()))
                .ReturnsAsync(response);

            var result = _validator.ValidateSuppliersAsync(suppliers);

            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();

            _mocker.GetMock<IPersonServiceClient>()
                .Verify(s => s.GetSuppliersAsync(It.IsAny<IEnumerable<long>>()), Times.Once());
        }

        [Fact]
        public void ValidateSuppliersAsync_WhenPersonServiceThrows_ThrowsUnexpectedException()
        {
            var suppliers = new List<long> { 1, 2 };

            _mocker.GetMock<IPersonServiceClient>()
                .Setup(s => s.GetSuppliersAsync(It.IsAny<IEnumerable<long>>()))
                .ThrowsAsync(new Exception("external failure"));

            Action act = () => _validator.ValidateSuppliersAsync(suppliers);

            act.Should().Throw<UnexpectedException>()
                .Where(ex => ex.Message.Contains("Erro inesperado os validar dados dos fornecedor"));

            _mocker.GetMock<IPersonServiceClient>()
                .Verify(s => s.GetSuppliersAsync(It.IsAny<IEnumerable<long>>()), Times.Once());
        }

        [Fact]
        public void ValidateSuppliersAsync_WhenResponseIsFallback_ReturnsFallbackValidationFailure()
        {
            var suppliers = new List<long> { 1, 2 };

            var response = new Response<IEnumerable<PersonResponseDTO>>()
            {
                Data = null,
                Success = false,
                Message = "Service fallback occurred"
            };

            _mocker.GetMock<IPersonServiceClient>()
                .Setup(s => s.GetSuppliersAsync(It.IsAny<IEnumerable<long>>()))
                .ReturnsAsync(response);

            var result = _validator.ValidateSuppliersAsync(suppliers);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Suppliers" &&
                                               e.ErrorMessage.Contains("Falha externa ao validar fornecedores. (fallback)"));

            _mocker.GetMock<IPersonServiceClient>()
                .Verify(s => s.GetSuppliersAsync(It.IsAny<IEnumerable<long>>()), Times.Once());
        }

        [Fact]
        public void ValidateSuppliersAsync_WhenAllSuppliersNotFound_ReturnsAllInvalidFailure()
        {
            var suppliers = new List<long> { 10, 20 };

            var response = new Response<IEnumerable<PersonResponseDTO>>(ErrorCodeEnum.NotFound);

            _mocker.GetMock<IPersonServiceClient>()
                .Setup(s => s.GetSuppliersAsync(It.IsAny<IEnumerable<long>>()))
                .ReturnsAsync(response);

            var result = _validator.ValidateSuppliersAsync(suppliers);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage == "Todos os fornecedores são inválidos");

            _mocker.GetMock<IPersonServiceClient>()
                .Verify(s => s.GetSuppliersAsync(It.IsAny<IEnumerable<long>>()), Times.Once());
        }

        [Fact]
        public void ValidateSuppliersAsync_WhenSomeInvalidSupplierIds_ReturnsInvalidIdsFailure()
        {
            var suppliers = new List<long> { 1, 2, 3 };

            var data = new List<PersonResponseDTO>
            {
                new PersonResponseDTO { Id = 1, Name = "A" },
                new PersonResponseDTO { Id = 3, Name = "C" }
            };

            var response = new Response<IEnumerable<PersonResponseDTO>>(data);

            _mocker.GetMock<IPersonServiceClient>()
                .Setup(s => s.GetSuppliersAsync(It.IsAny<IEnumerable<long>>()))
                .ReturnsAsync(response);

            var result = _validator.ValidateSuppliersAsync(suppliers);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Suppliers" &&
                                               e.ErrorMessage.Contains("Fornecedores inválidos:"))
                 .Which.ErrorMessage.Should().Contain("2");

            _mocker.GetMock<IPersonServiceClient>()
                .Verify(s => s.GetSuppliersAsync(It.IsAny<IEnumerable<long>>()), Times.Once());
        }
        #endregion
    }
}
