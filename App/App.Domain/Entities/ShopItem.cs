using System.ComponentModel.DataAnnotations;
using Base.Domain;
using Base.Domain.Contracts;

namespace App.Domain.Entities;

public class ShopItem: BaseDomainEntity, IDomainEntityId
{
    [MinLength(1)]
    [MaxLength(32)]
    [Required]
    public string Name { get; set; } = default!;
    [MinLength(1)]
    [MaxLength(512)]
    public string? Description { get; set; }
    [Range(0, int.MaxValue, ErrorMessage = "Value must not be negative.")]
    public int Price { get; set; } = default!;
    [Range(typeof(double), "1", "5")]
    public double Rating { get; set; } = default!;
    [Range(0, int.MaxValue, ErrorMessage = "Value must not be negative.")]
    public int StockAmount { get; set; } = default!;
    public byte[]? ItemPicture { get; set; }
    
    // Relationships
    public ICollection<ShoppingCartItem>? ShoppingCartItems { get; set; }
}