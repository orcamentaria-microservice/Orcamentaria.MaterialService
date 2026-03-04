using Bogus;
using Orcamentaria.Lib.Test.Fixtures;
using Orcamentaria.MaterialService.Domain.Models;
using Xunit;

namespace Orcamentaria.MaterialService.Test.Fixtures
{
    [CollectionDefinition(nameof(MaterialTypeCollection))]
    public class MaterialTypeCollection : ICollectionFixture<MaterialTypeFixture> { }

    public class MaterialTypeFixture : BaseFixture<MaterialType>
    {

        override
        public MaterialType CreateEntity(long id)
        {
            return new MaterialType
            {
                Id = id,
                Name = Faker.Name.FirstName(),
                Active = true,
                CompanyId = 1,
                CreatedAt = Faker.Date.Past(),
                CreatedBy = 1,
                UpdatedAt = Faker.Date.Future(),
                UpdatedBy = 1
            };
        }
    }
}
