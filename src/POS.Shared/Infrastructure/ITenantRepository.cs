using System.Linq.Expressions;
using POS.Shared.Domain;

namespace POS.Shared.Infrastructure;

public interface ITenantRepository<T> : IRepository<T> where T : TenantEntity
{
    Task<T?> GetByIdAsync(string id, string organizationId, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(string organizationId, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, string organizationId, CancellationToken cancellationToken = default);
}