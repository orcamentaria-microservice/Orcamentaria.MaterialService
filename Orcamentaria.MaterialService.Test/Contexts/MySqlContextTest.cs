using Microsoft.EntityFrameworkCore;
using Orcamentaria.MaterialService.Infrastructure.Contexts;

namespace Orcamentaria.MaterialService.Test.Contexts
{
    public class MySqlContextTest : MySqlContext
    {
        public MySqlContextTest(DbContextOptions<DbContext> options)
            : base(options) { }
    }
}
