using App.Domain.Entities;
using AutoMapper;
using Public.DTO.v1.ShoppingCartItems;

namespace App.BLL;

public class AutomapperConfig : Profile
{
    public AutomapperConfig()
    {
        CreateMap<ShopItem, ShopItemDetails>().ReverseMap();
        CreateMap<ShopItem, ShopItemListElement>().ReverseMap();
    }
}