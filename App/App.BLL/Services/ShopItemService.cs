using System.ComponentModel;
using App.BLL.Contracts;
using App.BLL.Mappers;
using App.DAL.Contracts;
using App.Domain.Entities;
using Base.BLL;
using Public.DTO.v1.ShoppingCartItems;

namespace App.BLL.Services;

public class ShopItemService(IAppUOW uow, ShopItemDetailsMapper itemDetailsMapper,
        ShopItemListElemMapper itemListElementMapper)
    : BaseEntityService<ShopItemDetails, ShopItem, IShopItemRepository>(uow.ShopItemRepository, itemDetailsMapper),
        IShopItemService
{
    public async Task<IEnumerable<ShopItemListElement?>> AllAsync(Guid? userId)
    {
        IEnumerable<ShopItem> items;

        if (userId == null)
        {
            // User is not authenticated, fetch all items without attaching quantity
            items = await uow.ShopItemRepository.AllAsync();
            return items.Select(e => itemListElementMapper.Map(e)).ToList();
        }

        // User is authenticated, fetch items along with their cart quantity for the user
        items = await uow.ShopItemRepository.GetCatalogItemsWithQuantityTaken((Guid)userId);
        return items.Select(e => itemListElementMapper.MapToShopItemListElem(e, (Guid)userId)).ToList();
    }

    public new async Task<ShopItemDetails?> FindAsync(Guid id)
        => itemDetailsMapper.Map(await uow.ShopItemRepository.FindAsync(id));
    
    
    public async Task<IEnumerable<ShopItemListElement?>> GetCartItems(Guid userId)
    {
        var cartItems = await uow.ShopItemRepository.GetCartItems(userId);
        return cartItems.Select(e => itemListElementMapper.MapToShopItemListElem(e.Item!, userId)).ToList();
    }

    public async Task<ShopItemDetails?> GetShopItem(Guid? userId, Guid itemId)
    {
        ShopItem? item;

        if (userId != null)
        {
            // User is authenticated, fetch all the item's data
            item = await uow.ShopItemRepository.GetCartItem((Guid)userId, itemId);
            if (item == null) throw new KeyNotFoundException("Item was not found.");
            return itemDetailsMapper.MapForAuthenticatedUser(item, (Guid)userId);
        }

        // User is not authenticated, send only the item data
        item = await uow.ShopItemRepository.FindAsync(itemId);
        return itemDetailsMapper.Map(item);
    }

    public async Task AddRemoveCartItem(Guid userId, Guid itemId, ECartItemActions action, int? quantity)
    {
        switch (action)
        {
            case ECartItemActions.Increment:
                await uow.ShopItemRepository
                    .AddCartItem(userId, itemId);
                break;
            case ECartItemActions.Decrement:
                await uow.ShopItemRepository
                    .RemoveCartItem(userId, itemId);
                break;
            case ECartItemActions.SetAmount:
                await uow.ShopItemRepository
                    .SetCartItemQuantity(userId, itemId, quantity);
                break;
            default:
                throw new InvalidEnumArgumentException("An invalid action has been chosen");
        }
    }

    public async Task RemoveAllCartItems(Guid userId) 
        => await uow.ShopItemRepository.RemoveAllCartItems(userId);
}