using FluentAssertions;
using Moq;
using Moq.AutoMock;
using Orcamentaria.Lib.Domain.Models;
using Orcamentaria.Lib.Domain.Models.Responses;
using Orcamentaria.MaterialService.Application.Validators;
using Orcamentaria.MaterialService.Domain.Models;
using Orcamentaria.MaterialService.Domain.Repositories;
using Orcamentaria.MaterialService.Test.Fixtures;
using System.Linq.Expressions;
using Xunit;

namespace Orcamentaria.MaterialService.Test.Validators
{
    [Collection(nameof(MaterialTypeCollection))]
    public class MaterialTypeValidatorTest
    {
        private readonly MaterialTypeFixture _fixture;
        private readonly AutoMocker _mocker;
        private readonly MaterialTypeValidator _validator;

        public MaterialTypeValidatorTest(MaterialTypeFixture fixture)
        {
            _fixture = fixture;
            _mocker = new AutoMocker();
            _validator = _mocker.CreateInstance<MaterialTypeValidator>(true);
        }

        #region ValidateBeforeInsert

        [Fact]
        public void ValidateBeforeInsert_WhenEntityValid_ReturnsIsValid()
        {
            var entity = _fixture.CreateEntity(0);

            _mocker.GetMock<IMaterialTypeRepository<MaterialType>>()
                .Setup(r => r.GetAsync(It.IsAny<GridParams>(), It.IsAny<Expression<Func<MaterialType, object>>[]>()))
                .ReturnsAsync((new List<MaterialType>(), new ResponsePagination(1, 10, 0)));

            var result = _validator.ValidateBeforeInsert(entity);

            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();

            _mocker.GetMock<IMaterialTypeRepository<MaterialType>>()
                .Verify(r => r.GetAsync(It.IsAny<GridParams>(), It.IsAny<Expression<Func<MaterialType, object>>[]>()), Times.Once());
        }

        [Fact]
        public void ValidateBeforeInsert_WhenIdIsInformed_ReturnsInvalidAndErrorMessage()
        {
            var entity = _fixture.CreateEntity(1);

            _mocker.GetMock<IMaterialTypeRepository<MaterialType>>()
                .Setup(r => r.GetAsync(It.IsAny<GridParams>(), It.IsAny<Expression<Func<MaterialType, object>>[]>()))
                .ReturnsAsync((new List<MaterialType>(), new ResponsePagination(1, 10, 0)));

            var result = _validator.ValidateBeforeInsert(entity);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => x.ErrorMessage == "O Id não deve ser informado.");

            _mocker.GetMock<IMaterialTypeRepository<MaterialType>>()
                .Verify(r => r.GetAsync(It.IsAny<GridParams>(), It.IsAny<Expression<Func<MaterialType, object>>[]>()), Times.Once());
        }

        [Fact]
        public void ValidateBeforeInsert_WhenNameIsEmpty_ReturnsInvalidAndErrorMessage()
        {
            var entity = _fixture.CreateEntity(0);
            entity.Name = string.Empty;

            _mocker.GetMock<IMaterialTypeRepository<MaterialType>>()
                .Setup(r => r.GetAsync(It.IsAny<GridParams>(), It.IsAny<Expression<Func<MaterialType, object>>[]>()))
                .ReturnsAsync((new List<MaterialType>(), new ResponsePagination(1, 10, 0)));

            var result = _validator.ValidateBeforeInsert(entity);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => x.ErrorMessage == "O Name é obrigatório.");

            _mocker.GetMock<IMaterialTypeRepository<MaterialType>>()
                .Verify(r => r.GetAsync(It.IsAny<GridParams>(), It.IsAny<Expression<Func<MaterialType, object>>[]>()), Times.Once());
        }

        [Fact]
        public void ValidateBeforeInsert_WhenNameIsNull_ReturnsInvalidAndErrorMessage()
        {
            var entity = _fixture.CreateEntity(0);
            entity.Name = null;

            _mocker.GetMock<IMaterialTypeRepository<MaterialType>>()
                .Setup(r => r.GetAsync(It.IsAny<GridParams>(), It.IsAny<Expression<Func<MaterialType, object>>[]>()))
                .ReturnsAsync((new List<MaterialType>(), new ResponsePagination(1, 10, 0)));

            var result = _validator.ValidateBeforeInsert(entity);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => x.ErrorMessage == "O Name é obrigatório.");

            _mocker.GetMock<IMaterialTypeRepository<MaterialType>>()
                .Verify(r => r.GetAsync(It.IsAny<GridParams>(), It.IsAny<Expression<Func<MaterialType, object>>[]>()), Times.Once());
        }

        [Fact]
        public void ValidateBeforeInsert_WhenNameLengthGreaterThan40Caracters_ReturnsInvalidAndErrorMessage()
        {
            var entity = _fixture.CreateEntity(0);
            entity.Name = _fixture.Faker.Random.AlphaNumeric(41);

            _mocker.GetMock<IMaterialTypeRepository<MaterialType>>()
                .Setup(r => r.GetAsync(It.IsAny<GridParams>(), It.IsAny<Expression<Func<MaterialType, object>>[]>()))
                .ReturnsAsync((new List<MaterialType>(), new ResponsePagination(1, 10, 0)));

            var result = _validator.ValidateBeforeInsert(entity);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => x.ErrorMessage == "O tamanho máximo do Name é de 40 caracteres.");

            _mocker.GetMock<IMaterialTypeRepository<MaterialType>>()
                .Verify(r => r.GetAsync(It.IsAny<GridParams>(), It.IsAny<Expression<Func<MaterialType, object>>[]>()), Times.Once());
        }

        [Fact]
        public void ValidateBeforeInsert_WhenNameAlreadyExists_ReturnsInvalidAndErrorMessage()
        {
            var entity = _fixture.CreateEntity(0);
            var existing = _fixture.CreateEntity(2);

            var data = new List<MaterialType> { existing };

            _mocker.GetMock<IMaterialTypeRepository<MaterialType>>()
                .Setup(r => r.GetAsync(It.IsAny<GridParams>(), It.IsAny<Expression<Func<MaterialType, object>>[]>()))
                .ReturnsAsync((data, new ResponsePagination(1, 10, 1)));

            var result = _validator.ValidateBeforeInsert(entity);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => x.ErrorMessage == "Esse tipo já existe.");

            _mocker.GetMock<IMaterialTypeRepository<MaterialType>>()
                .Verify(r => r.GetAsync(It.IsAny<GridParams>(), It.IsAny<Expression<Func<MaterialType, object>>[]>()), Times.Once());
        }

        #endregion

        #region ValidateBeforeUpdate

        [Fact]
        public void ValidateBeforeUpdate_WhenEntityValid_ReturnsIsValid()
        {
            var entity = _fixture.CreateEntity(1);

            _mocker.GetMock<IMaterialTypeRepository<MaterialType>>()
                .Setup(r => r.GetByIdAsync(It.IsAny<long>(), It.IsAny<Expression<Func<MaterialType, object>>[]>()))
                .ReturnsAsync(entity);

            _mocker.GetMock<IMaterialTypeRepository<MaterialType>>()
                .Setup(r => r.GetAsync(It.IsAny<GridParams>(), It.IsAny<Expression<Func<MaterialType, object>>[]>()))
                .ReturnsAsync((new List<MaterialType>(), new ResponsePagination(1, 10, 0)));

            var result = _validator.ValidateBeforeUpdate(entity);

            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();

            _mocker.GetMock<IMaterialTypeRepository<MaterialType>>()
                .Verify(r => r.GetByIdAsync(It.IsAny<long>(), It.IsAny<Expression<Func<MaterialType, object>>[]>()), Times.Once());

            _mocker.GetMock<IMaterialTypeRepository<MaterialType>>()
                .Verify(r => r.GetAsync(It.IsAny<GridParams>(), It.IsAny<Expression<Func<MaterialType, object>>[]>()), Times.Once());
        }

        [Fact]
        public void ValidateBeforeUpdate_WhenIdThanZero_ReturnsInvalidAndErrorMessage()
        {
            var entity = _fixture.CreateEntity(0);

            _mocker.GetMock<IMaterialTypeRepository<MaterialType>>()
                .Setup(r => r.GetByIdAsync(It.IsAny<long>(), It.IsAny<Expression<Func<MaterialType, object>>[]>()))
                .ReturnsAsync((MaterialType)null);

            _mocker.GetMock<IMaterialTypeRepository<MaterialType>>()
                .Setup(r => r.GetAsync(It.IsAny<GridParams>(), It.IsAny<Expression<Func<MaterialType, object>>[]>()))
                .ReturnsAsync((new List<MaterialType>(), new ResponsePagination(1, 10, 0)));

            var result = _validator.ValidateBeforeUpdate(entity);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => x.ErrorMessage == "O Id deve ser informado.");

            _mocker.GetMock<IMaterialTypeRepository<MaterialType>>()
                .Verify(r => r.GetByIdAsync(It.IsAny<long>(), It.IsAny<Expression<Func<MaterialType, object>>[]>()), Times.Once());

            _mocker.GetMock<IMaterialTypeRepository<MaterialType>>()
                .Verify(r => r.GetAsync(It.IsAny<GridParams>(), It.IsAny<Expression<Func<MaterialType, object>>[]>()), Times.Once());
        }

        [Fact]
        public void ValidateBeforeUpdate_WhenIdNotFound_ReturnsInvalidAndErrorMessage()
        {
            var entity = _fixture.CreateEntity(52);

            _mocker.GetMock<IMaterialTypeRepository<MaterialType>>()
                .Setup(r => r.GetByIdAsync(It.IsAny<long>(), It.IsAny<Expression<Func<MaterialType, object>>[]>()))
                .ReturnsAsync((MaterialType)null);

            _mocker.GetMock<IMaterialTypeRepository<MaterialType>>()
                .Setup(r => r.GetAsync(It.IsAny<GridParams>(), It.IsAny<Expression<Func<MaterialType, object>>[]>()))
                .ReturnsAsync((new List<MaterialType>(), new ResponsePagination(1, 10, 0)));

            var result = _validator.ValidateBeforeUpdate(entity);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => x.ErrorMessage == "Id não encontrado.");

            _mocker.GetMock<IMaterialTypeRepository<MaterialType>>()
                .Verify(r => r.GetByIdAsync(It.IsAny<long>(), It.IsAny<Expression<Func<MaterialType, object>>[]>()), Times.Once());

            _mocker.GetMock<IMaterialTypeRepository<MaterialType>>()
                .Verify(r => r.GetAsync(It.IsAny<GridParams>(), It.IsAny<Expression<Func<MaterialType, object>>[]>()), Times.Once());
        }

        [Fact]
        public void ValidateBeforeUpdate_WhenNameAlreadyExists_ReturnsInvalidAndErrorMessage()
        {
            var entity = _fixture.CreateEntity(1);
            var other = _fixture.CreateEntity(2);
            other.Name = entity.Name; // same name but different id => should be invalid

            var data = new List<MaterialType> { other };

            _mocker.GetMock<IMaterialTypeRepository<MaterialType>>()
                .Setup(r => r.GetByIdAsync(It.IsAny<long>(), It.IsAny<Expression<Func<MaterialType, object>>[]>()))
                .ReturnsAsync(entity);

            _mocker.GetMock<IMaterialTypeRepository<MaterialType>>()
                .Setup(r => r.GetAsync(It.IsAny<GridParams>(), It.IsAny<Expression<Func<MaterialType, object>>[]>()))
                .ReturnsAsync((data, new ResponsePagination(1, 10, 1)));

            var result = _validator.ValidateBeforeUpdate(entity);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => x.ErrorMessage == "Esse tipo já existe.");

            _mocker.GetMock<IMaterialTypeRepository<MaterialType>>()
                .Verify(r => r.GetByIdAsync(It.IsAny<long>(), It.IsAny<Expression<Func<MaterialType, object>>[]>()), Times.Once());

            _mocker.GetMock<IMaterialTypeRepository<MaterialType>>()
                .Verify(r => r.GetAsync(It.IsAny<GridParams>(), It.IsAny<Expression<Func<MaterialType, object>>[]>()), Times.Once());
        }

        [Fact]
        public void ValidateBeforeUpdate_WhenNameBelongsToSameEntity_ReturnsValid()
        {
            var entity = _fixture.CreateEntity(1);

            var data = new List<MaterialType> { entity };

            _mocker.GetMock<IMaterialTypeRepository<MaterialType>>()
                .Setup(r => r.GetByIdAsync(It.IsAny<long>(), It.IsAny<Expression<Func<MaterialType, object>>[]>()))
                .ReturnsAsync(entity);

            _mocker.GetMock<IMaterialTypeRepository<MaterialType>>()
                .Setup(r => r.GetAsync(It.IsAny<GridParams>(), It.IsAny<Expression<Func<MaterialType, object>>[]>()))
                .ReturnsAsync((data, new ResponsePagination(1, 10, 1)));

            var result = _validator.ValidateBeforeUpdate(entity);

            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();

            _mocker.GetMock<IMaterialTypeRepository<MaterialType>>()
                .Verify(r => r.GetByIdAsync(It.IsAny<long>(), It.IsAny<Expression<Func<MaterialType, object>>[]>()), Times.Once());

            _mocker.GetMock<IMaterialTypeRepository<MaterialType>>()
                .Verify(r => r.GetAsync(It.IsAny<GridParams>(), It.IsAny<Expression<Func<MaterialType, object>>[]>()), Times.Once());
        }

        [Fact]
        public void ValidateBeforeUpdate_WhenNameIsEmpty_ReturnsInvalidAndErrorMessage()
        {
            var entity = _fixture.CreateEntity(1);
            entity.Name = string.Empty;

            _mocker.GetMock<IMaterialTypeRepository<MaterialType>>()
                .Setup(r => r.GetByIdAsync(It.IsAny<long>(), It.IsAny<Expression<Func<MaterialType, object>>[]>()))
                .ReturnsAsync(entity);

            _mocker.GetMock<IMaterialTypeRepository<MaterialType>>()
                .Setup(r => r.GetAsync(It.IsAny<GridParams>(), It.IsAny<Expression<Func<MaterialType, object>>[]>()))
                .ReturnsAsync((new List<MaterialType>(), new ResponsePagination(1, 10, 0)));

            var result = _validator.ValidateBeforeUpdate(entity);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => x.ErrorMessage == "O Name é obrigatório.");

            _mocker.GetMock<IMaterialTypeRepository<MaterialType>>()
                .Verify(r => r.GetByIdAsync(It.IsAny<long>(), It.IsAny<Expression<Func<MaterialType, object>>[]>()), Times.Once());

            _mocker.GetMock<IMaterialTypeRepository<MaterialType>>()
                .Verify(r => r.GetAsync(It.IsAny<GridParams>(), It.IsAny<Expression<Func<MaterialType, object>>[]>()), Times.Once());
        }

        [Fact]
        public void ValidateBeforeUpdate_WhenNameLengthGreaterThan40Caracters_ReturnsInvalidAndErrorMessage()
        {
            var entity = _fixture.CreateEntity(1);
            entity.Name = _fixture.Faker.Random.AlphaNumeric(41);

            _mocker.GetMock<IMaterialTypeRepository<MaterialType>>()
                .Setup(r => r.GetByIdAsync(It.IsAny<long>(), It.IsAny<Expression<Func<MaterialType, object>>[]>()))
                .ReturnsAsync(entity);

            _mocker.GetMock<IMaterialTypeRepository<MaterialType>>()
                .Setup(r => r.GetAsync(It.IsAny<GridParams>(), It.IsAny<Expression<Func<MaterialType, object>>[]>()))
                .ReturnsAsync((new List<MaterialType>(), new ResponsePagination(1, 10, 0)));

            var result = _validator.ValidateBeforeUpdate(entity);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(x => x.ErrorMessage == "O tamanho máximo do Name é de 40 caracteres.");

            _mocker.GetMock<IMaterialTypeRepository<MaterialType>>()
                .Verify(r => r.GetByIdAsync(It.IsAny<long>(), It.IsAny<Expression<Func<MaterialType, object>>[]>()), Times.Once());

            _mocker.GetMock<IMaterialTypeRepository<MaterialType>>()
                .Verify(r => r.GetAsync(It.IsAny<GridParams>(), It.IsAny<Expression<Func<MaterialType, object>>[]>()), Times.Once());
        }

        #endregion
    }
}
