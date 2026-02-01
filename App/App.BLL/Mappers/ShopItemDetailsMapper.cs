using App.Domain.Entities;
using AutoMapper;
using Base.Mapper;
using Public.DTO.v1.ShoppingCartItems;

namespace App.BLL.Mappers;

public class ShopItemDetailsMapper(IMapper mapper) : BaseMapper<ShopItemDetails, ShopItem>(mapper)
{
    public ShopItemDetails? MapForAuthenticatedUser(ShopItem shopItem, Guid userId)
    {
        // Map the base shop item properties
        var shopItemDetails = Mapper.Map<ShopItemDetails>(shopItem);

        // If the item and the user's cart items exist, set the quantity taken
        if (shopItemDetails != null && shopItem.ShoppingCartItems != null)
        {
            var shoppingCartItem = shopItem.ShoppingCartItems.FirstOrDefault(e => e.AppUserId == userId);
            if (shoppingCartItem != null)
            {
                // Attach the quantity from the user's cart to the DTO
                shopItemDetails.QuantityTaken = shoppingCartItem.Quantity;
            }
        }
    
        return shopItemDetails;
    }
}