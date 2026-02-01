using Base.Mapper.Contracts;

namespace Base.Mapper;

public class BaseMapper<TSource, TDestination>(AutoMapper.IMapper mapper) : IMapper<TSource, TDestination>
{
    protected readonly AutoMapper.IMapper Mapper = mapper;

    public virtual TSource? Map(TDestination? entity)
    {
        return Mapper.Map<TSource>(entity);
    }

    public virtual TDestination? Map(TSource? entity)
    {
        return Mapper.Map<TDestination>(entity);
    }
}