using Base.BLL.Contracts;
using Base.Domain.Contracts;

namespace Base.BLL;

// ReSharper disable once InconsistentNaming
public abstract class BaseBLL<TUow>(TUow uow) : IBaseBLL
    where TUow : IBaseUOW
{
    protected readonly TUow Uow = uow;

    public virtual async Task<int> SaveChangesAsync() 
        => await Uow.SaveChangesAsync();
}