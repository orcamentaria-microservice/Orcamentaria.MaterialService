using Orcamentaria.Lib.Test.Repositories;
using Orcamentaria.MaterialService.Domain.Models;
using Orcamentaria.MaterialService.Test.Contexts;
using Orcamentaria.MaterialService.Test.Fixtures;
using Xunit;

namespace Orcamentaria.MaterialService.Test.Repositories
{
    [Collection(nameof(MaterialSupplierCollection))]
    public class MaterialSupplierReadRepositoryTest : ReadWithoutCompanyRepositoryTests<MaterialSupplier, MySqlContextTest>
    {
        public MaterialSupplierReadRepositoryTest(MaterialSupplierFixture fixture) : base(fixture) { }
    }
}
