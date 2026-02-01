using System.ComponentModel.DataAnnotations;
using App.Domain.Entities;
using Base.Domain.Contracts;
using Microsoft.AspNetCore.Identity;

namespace App.Domain.Identity;

public class AppUser : IdentityUser<Guid>, IDomainEntityId
{
    [MinLength(1)]
    [MaxLength(32)]
    public string FirstName { get; set; } = default!;
    [MinLength(1)]
    [MaxLength(32)]
    public string LastName { get; set; } = default!;
    
    // Relationships
    public ICollection<ShoppingCartItem>? ShoppingCartItems { get; set; }
    public ICollection<AppRefreshToken>? AppRefreshTokens { get; set; }
}