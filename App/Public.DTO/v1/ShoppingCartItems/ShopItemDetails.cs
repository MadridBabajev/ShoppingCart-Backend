using Public.DTO.v1.BaseDTO;

namespace Public.DTO.v1.ShoppingCartItems;

/// <summary>
/// Represents the details DTO an the item.
/// </summary>
public class ShopItemDetails: ItemDtoBase
{
    /// <summary>
    /// Description of the item.
    /// </summary>
    public string? Description { get; set; } = default!;
}