using App.Domain.Entities;
using Base.DAL.Contract;

namespace App.DAL.Contracts;

public interface IShopItemRepository : IBaseRepository<ShopItem>
{
    // Repo custom methods can be added here
    Task AddCartItem(Guid userId, Guid itemId);
    Task RemoveCartItem(Guid userId, Guid itemId);
    Task SetCartItemQuantity(Guid userId, Guid itemId, int? quantity);
    Task RemoveAllCartItems(Guid userId);
    Task<IEnumerable<ShoppingCartItem>> GetCartItems(Guid userId);
    Task<ShopItem?> GetCartItem(Guid userId, Guid itemId);
    Task<List<ShopItem>> GetCatalogItemsWithQuantityTaken(Guid userId);
}