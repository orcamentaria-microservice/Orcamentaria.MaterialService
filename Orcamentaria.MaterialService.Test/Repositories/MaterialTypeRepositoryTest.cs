using Orcamentaria.Lib.Test.Repositories;
using Orcamentaria.MaterialService.Domain.Models;
using Orcamentaria.MaterialService.Test.Contexts;
using Orcamentaria.MaterialService.Test.Fixtures;
using Xunit;

namespace Orcamentaria.MaterialService.Test.Repositories
{
    [Collection(nameof(MaterialTypeCollection))]
    public class MaterialTypeReadRepositoryTest : ReadWithCompanyRepositoryTests<MaterialType, MySqlContextTest>
    {
        public MaterialTypeReadRepositoryTest(MaterialTypeFixture fixture) : base(fixture) { }
    }

    [Collection(nameof(MaterialTypeCollection))]
    public class MaterialTypeWriteRepositoryTest : WriteWithCompanyRepositoryTests<MaterialType, MySqlContextTest>
    {
        public MaterialTypeWriteRepositoryTest(MaterialTypeFixture fixture) : base(fixture) { }
    }

    [Collection(nameof(MaterialTypeCollection))]
    public class MaterialTypeDeleteRepositoryTest : DeleteWithCompanyRepositoryTests<MaterialType, MySqlContextTest>
    {
        public MaterialTypeDeleteRepositoryTest(MaterialTypeFixture fixture) : base(fixture) { }
    }
}
