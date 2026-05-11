using System.Linq.Expressions;
using Hardware.Domain.Common;
using Hardware.Domain.Interfaces.Repositories;
using Hardware.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Hardware.Infrastructure.Repositories;

public class GenericRepository<T>(ApplicationDbContext context) : IGenericRepository<T> where T : BaseEntity
{
    private readonly ApplicationDbContext _context = context;
    private readonly DbSet<T> _set = context.Set<T>();

    public Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _set.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default) =>
        _set.FirstOrDefaultAsync(predicate, cancellationToken);

    public async Task<IReadOnlyList<T>> ListAsync(CancellationToken cancellationToken = default) =>
        await _set.AsNoTracking().ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<T>> ListAsync(Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default) =>
        await _set.AsNoTracking().Where(predicate).ToListAsync(cancellationToken);

    public Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default) =>
        _set.AnyAsync(predicate, cancellationToken);

    public Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default) =>
        predicate is null ? _set.CountAsync(cancellationToken) : _set.CountAsync(predicate, cancellationToken);

    public IQueryable<T> Query(bool tracking = false) =>
        tracking ? _set : _set.AsNoTracking();

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default) =>
        await _set.AddAsync(entity, cancellationToken);

    public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default) =>
        await _set.AddRangeAsync(entities, cancellationToken);

    public void Update(T entity) => _set.Update(entity);

    public void Delete(T entity) => _set.Remove(entity);

    public void DeleteRange(IEnumerable<T> entities) => _set.RemoveRange(entities);
}
