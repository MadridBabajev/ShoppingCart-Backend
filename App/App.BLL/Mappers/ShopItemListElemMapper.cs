using App.Domain.Entities;
using AutoMapper;
using Base.Mapper;
using Public.DTO.v1.ShoppingCartItems;

namespace App.BLL.Mappers;

public class ShopItemListElemMapper(IMapper mapper) : BaseMapper<ShopItemListElement, ShopItem>(mapper)
{
    public ShopItemListElement? MapToShopItemListElem(ShopItem shopItem, Guid userId)
    {
        // Map the base shop item properties
        var shopItemListElem = Mapper.Map<ShopItemListElement>(shopItem);

        // If the item and the user's cart items exist, set the quantity taken
        if (shopItemListElem != null && shopItem.ShoppingCartItems != null)
        {
            var shoppingCartItem = shopItem.ShoppingCartItems.FirstOrDefault(e => e.AppUserId == userId);
            if (shoppingCartItem != null)
            {
                // Attach the quantity from the user's cart to the DTO
                shopItemListElem.QuantityTaken = shoppingCartItem.Quantity;
            }
        }
    
        return shopItemListElem;
    }
}