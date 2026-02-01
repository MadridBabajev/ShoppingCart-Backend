using System.ComponentModel.DataAnnotations;
using App.Domain.Identity;
using Base.Domain;
using Base.Domain.Contracts;

namespace App.Domain.Entities;

public class ShoppingCartItem : BaseDomainEntity, IDomainEntityId
{
    [Range(0, int.MaxValue, ErrorMessage = "Value must not be negative.")]
    public int Quantity { get; set; } = 1;

    // Relationships
    public Guid ItemId { get; set; }
    public ShopItem? Item { get; set; }

    public Guid AppUserId { get; set; }
    public AppUser? AppUser { get; set; }
}