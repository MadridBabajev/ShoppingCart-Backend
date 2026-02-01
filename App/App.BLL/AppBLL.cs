using App.BLL.Contracts;
using App.BLL.Mappers;
using App.BLL.Services;
using App.DAL.Contracts;
using AutoMapper;
using Base.BLL;

namespace App.BLL;

// ReSharper disable InconsistentNaming
public class AppBLL(IAppUOW uow, IMapper mapper) : BaseBLL<IAppUOW>(uow), IAppBLL
{
    protected new readonly IAppUOW Uow = uow;

    // services
    private IShopItemService? _shopItems;

    public IShopItemService ShopItemService =>
        _shopItems ??= new ShopItemService(Uow,
            new ShopItemDetailsMapper(mapper), 
            new ShopItemListElemMapper(mapper));
}