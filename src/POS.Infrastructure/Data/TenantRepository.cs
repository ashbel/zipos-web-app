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

    // Database-per-tenant: same behavior; the active DbContext points to the tenant database
    public async Task<T?> GetByIdAsync(string id, string organizationId, CancellationToken cancellationToken = default)
    {
        // organizationId parameter is not used; isolation via connection routing
        return await _dbSet.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<T>> GetAllAsync(string organizationId, CancellationToken cancellationToken = default)
    {
        // organizationId parameter is not used; isolation via connection routing
        return await _dbSet.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, string organizationId, CancellationToken cancellationToken = default)
    {
        // organizationId parameter is not used; isolation via connection routing
        return await _dbSet.Where(predicate).ToListAsync(cancellationToken);
    }
}