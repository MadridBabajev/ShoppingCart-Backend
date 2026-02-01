using Base.BLL.Contracts;
using Base.DAL.Contract;
using Base.Domain.Contracts;
using Base.Mapper.Contracts;

namespace Base.BLL;

public class BaseEntityService<TBllEntity, TDalEntity, TRepository> :
    BaseEntityService<TBllEntity, TDalEntity, TRepository, Guid>, IEntityService<TBllEntity>
    where TBllEntity : class, IDomainEntityId
    where TDalEntity : class, IDomainEntityId
    where TRepository : IBaseRepository<TDalEntity>
{
    public BaseEntityService(TRepository repository, IMapper<TBllEntity, TDalEntity> mapper) : base(repository, mapper)
    {
    }
}

public class BaseEntityService<TBllEntity, TDalEntity, TRepository, TKey>(TRepository repository,
        IMapper<TBllEntity, TDalEntity> mapper)
    : IEntityService<TBllEntity, TKey>
    where TBllEntity : class, IDomainEntityId<TKey>
    where TDalEntity : class, IDomainEntityId<TKey>
    where TRepository : IBaseRepository<TDalEntity, TKey>
    where TKey : struct, IEquatable<TKey>
{
    private readonly TRepository _repository = repository;
    private readonly IMapper<TBllEntity, TDalEntity> _mapper = mapper;

    public async Task<IEnumerable<TBllEntity>> AllAsync()
    {
        return (await _repository.AllAsync()).Select(e => _mapper.Map(e))!;
    }

    public async Task<TBllEntity?> FindAsync(TKey id)
    {
        return _mapper.Map(await _repository.FindAsync(id));
    }
}