using App.DAL.Contracts;
using App.DAL.Repositories;
using Base.DAL;

namespace App.DAL;

// ReSharper disable once InconsistentNaming
public class AppUOW(ApplicationDbContext dataContext) : EFBaseUOW<ApplicationDbContext>(dataContext), IAppUOW
{
    private IShopItemRepository? _shopItemsRepository;
    public IShopItemRepository ShopItemRepository =>
        _shopItemsRepository ??= new ShopItemRepository(UowDbContext);
}