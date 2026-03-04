using Bogus;
using Microsoft.EntityFrameworkCore;
using Orcamentaria.Lib.Test.Fixtures;
using Orcamentaria.MaterialService.Domain.Models;
using Orcamentaria.MaterialService.Test.Contexts;
using Polly;
using Xunit;

namespace Orcamentaria.MaterialService.Test.Fixtures
{
    [CollectionDefinition(nameof(MaterialCollection))]
    public class MaterialCollection : ICollectionFixture<MaterialFixture> { }


    public class MaterialFixture : BaseFixture<Material>
    {

        override
        public Material CreateEntity(long id)
        {
            return new Material
            {
                Id = id,
                Name = Faker.Name.FirstName(),
                Description = Faker.Commerce.ProductDescription(),
                Active = true,
                Manufacturer = Faker.Name.LastName(),
                TypeId = 1,
                CompanyId = 1,
                CreatedAt = Faker.Date.Past(),
                CreatedBy = 1,
                UpdatedAt = Faker.Date.Future(),
                UpdatedBy = 1
            };
        }

        public async Task AddSuppliers(MySqlContextTest _dbContext, long materialId, IEnumerable<MaterialSupplier> suppliers)
        {
            var materialEntity = _dbContext.Materials
                    .Include(x => x.Suppliers)
                    .First(x => x.Id == materialId);

            materialEntity.Suppliers = materialEntity.Suppliers.Union(suppliers).ToList();

            await _dbContext.SaveChangesAsync();
        }
    }
}
