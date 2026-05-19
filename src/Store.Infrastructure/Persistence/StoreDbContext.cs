using Microsoft.EntityFrameworkCore;

namespace Store.Infrastructure.Persistence;

public sealed class StoreDbContext(DbContextOptions<StoreDbContext> options) : DbContext(options)
{
}
