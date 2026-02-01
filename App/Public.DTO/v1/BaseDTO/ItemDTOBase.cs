using Base.Domain.Contracts;

namespace Public.DTO.v1.BaseDTO;

public class ItemDtoBase: IDomainEntityId
{
    /// <summary>
    /// ID of the item.
    /// </summary>
    public Guid Id { get; set; }
    /// <summary>
    /// Name of the item.
    /// </summary>
    public string Name { get; set; } = default!;
    /// <summary>
    /// Price of the item.
    /// </summary>
    public int Price { get; set; } = default!;
    /// <summary>
    /// Rating of the item.
    /// </summary>
    public double Rating { get; set; } = default!;
    /// <summary>
    /// Picture of the item (if present).
    /// </summary>
    public byte[]? ItemPicture { get; set; }
    /// <summary>
    /// Quantity of the item in cart.
    /// </summary>
    public int? QuantityTaken { get; set; } = default!;
    /// <summary>
    /// Quantity of the item in stock.
    /// </summary>
    public int? StockAmount { get; set; } = default!;
}