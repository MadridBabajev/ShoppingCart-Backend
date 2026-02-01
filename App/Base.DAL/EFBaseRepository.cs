using Base.DAL.Contract;
using Base.Domain.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Base.DAL;

// ReSharper disable InconsistentNaming
public abstract class EFBaseRepository<TEntity, TDbContext>: 
    EFBaseRepository<TEntity, Guid, TDbContext>, 
    IBaseRepository<TEntity>
    where TEntity: class, IDomainEntityId
    where TDbContext: DbContext
{
    protected EFBaseRepository(TDbContext dataContext) : base(dataContext)
    {
    }
}

public abstract class EFBaseRepository<TEntity, TKey, TDbContext> : IBaseRepository<TEntity, TKey>
    where TEntity : class, IDomainEntityId<TKey>
    where TKey : struct, IEquatable<TKey>
    where TDbContext : DbContext
{
    protected TDbContext RepositoryDbContext;
    protected DbSet<TEntity> RepositoryDbSet;

    public EFBaseRepository(TDbContext dataContext)
    {
        RepositoryDbContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
        RepositoryDbSet = RepositoryDbContext.Set<TEntity>();
    }

    public virtual async Task<IEnumerable<TEntity>> AllAsync()
        =>  await RepositoryDbSet.ToListAsync();

    public virtual async Task<TEntity?> FindAsync(TKey id)
        =>  await RepositoryDbSet.FindAsync(id);
}