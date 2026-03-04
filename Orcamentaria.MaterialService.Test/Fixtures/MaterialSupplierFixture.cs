using Orcamentaria.Lib.Test.Fixtures;
using Orcamentaria.MaterialService.Domain.Models;
using Xunit;

namespace Orcamentaria.MaterialService.Test.Fixtures
{
    [CollectionDefinition(nameof(MaterialSupplierCollection))]
    public class MaterialSupplierCollection : ICollectionFixture<MaterialSupplierFixture> { }

    public class MaterialSupplierFixture : BaseFixture<MaterialSupplier>
    {

        override
        public MaterialSupplier CreateEntity(long id)
        {
            return new MaterialSupplier
            {
               Id = id,
               MaterialId = Faker.Random.Long(1, 99999),
               SupplierId = Faker.Random.Long(1, 99999),
               CompanyId = 1,
            };
        }
    }
}
