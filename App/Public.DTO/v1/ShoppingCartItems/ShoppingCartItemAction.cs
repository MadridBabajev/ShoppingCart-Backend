namespace Public.DTO.v1.ShoppingCartItems;

public class ShoppingCartItemAction
{
    public Guid ItemId { get; set; }
    public ECartItemActions ItemAction { get; set; }
    public int? Quantity { get; set; }
}