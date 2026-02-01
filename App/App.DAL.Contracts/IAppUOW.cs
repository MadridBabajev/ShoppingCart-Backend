using Base.Domain.Contracts;

namespace App.DAL.Contracts;
// ReSharper disable once InconsistentNaming
public interface IAppUOW : IBaseUOW
{
    // list all the repositories
    IShopItemRepository ShopItemRepository { get; }
}