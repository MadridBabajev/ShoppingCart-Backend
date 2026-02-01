using Base.BLL.Contracts;
using Public.DTO.v1.ShoppingCartItems;

namespace App.BLL.Contracts;


public interface IShopItemService: IEntityService<ShopItemDetails>
{
    // custom methods
    Task<IEnumerable<ShopItemListElement?>> AllAsync(Guid? userId);
    Task AddRemoveCartItem(Guid userId, Guid itemId, ECartItemActions action, int? quantity);
    Task RemoveAllCartItems(Guid userId);
    Task<IEnumerable<ShopItemListElement?>> GetCartItems(Guid userId);
    public Task<ShopItemDetails?> GetShopItem(Guid? userId, Guid itemId);
}