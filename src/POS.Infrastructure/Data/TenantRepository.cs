using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using POS.Shared.Domain;
using POS.Shared.Infrastructure;

namespace POS.Infrastructure.Data;

public class TenantRepository<T> : Repository<T>, ITenantRepository<T> where T : TenantEntity
{
    public TenantRepository(POSDbContext context) : base(context)
    {
    }

    // With schema-based multi-tenancy, these methods work the same as the base repository
    // since the schema context automatically isolates tenant data
    public async Task<T?> GetByIdAsync(string id, string organizationId, CancellationToken cancellationToken = default)
    {
        // organizationId parameter is kept for interface compatibility but not needed
        // since schema isolation handles tenant separation
        return await _dbSet.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<T>> GetAllAsync(string organizationId, CancellationToken cancellationToken = default)
    {
        // organizationId parameter is kept for interface compatibility but not needed
        // since schema isolation handles tenant separation
        return await _dbSet.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, string organizationId, CancellationToken cancellationToken = default)
    {
        // organizationId parameter is kept for interface compatibility but not needed
        // since schema isolation handles tenant separation
        return await _dbSet.Where(predicate).ToListAsync(cancellationToken);
    }
}