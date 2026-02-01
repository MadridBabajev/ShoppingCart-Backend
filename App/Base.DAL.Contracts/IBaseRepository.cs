using Base.Domain.Contracts;

namespace Base.DAL.Contract;

public interface IBaseRepository<TEntity>: IBaseRepository<TEntity, Guid>
    where TEntity: class, IDomainEntityId;

public interface IBaseRepository<TEntity, in TKey>
    where TEntity : class, IDomainEntityId<TKey>
    where TKey : struct, IEquatable<TKey>
{
    Task<IEnumerable<TEntity>> AllAsync();
    Task<TEntity?> FindAsync(TKey id);
}
