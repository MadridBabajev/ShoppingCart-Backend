using Base.Domain.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Base.DAL;

// ReSharper disable once InconsistentNaming
public abstract class EFBaseUOW<TDbContext>(TDbContext dataContext) : IBaseUOW
    where TDbContext : DbContext
{
    protected readonly TDbContext UowDbContext = dataContext;

    public virtual async Task<int> SaveChangesAsync()
    {
        return await UowDbContext.SaveChangesAsync();
    }
}