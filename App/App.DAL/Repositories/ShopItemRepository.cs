using App.DAL.Contracts;
using App.Domain.Entities;
using Base.DAL;
using Microsoft.EntityFrameworkCore;

namespace App.DAL.Repositories;

public class ShopItemRepository : EFBaseRepository<ShopItem, ApplicationDbContext>, IShopItemRepository
{
    public ShopItemRepository(ApplicationDbContext dataContext) : base(dataContext) { }
    
    public async Task<IEnumerable<ShoppingCartItem>> GetCartItems(Guid userId)
    {
        var user = await RepositoryDbContext.Users
            .Include(u => u.ShoppingCartItems)!
            .ThenInclude(i => i.Item)
            .FirstOrDefaultAsync(u => u.Id == userId);

        return user?.ShoppingCartItems ?? new List<ShoppingCartItem>();
    }
    
    public async Task<ShopItem?> GetCartItem(Guid userId, Guid itemId)
    {
        var user = await RepositoryDbContext.Users
            .Include(u => u.ShoppingCartItems)!
            .ThenInclude(ci => ci.Item)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            throw new KeyNotFoundException("User not found.");

        var cartItem = user.ShoppingCartItems?.FirstOrDefault(i => i.ItemId == itemId);

        return cartItem?.Item;
    }
    
    public async Task<List<ShopItem>> GetCatalogItemsWithQuantityTaken(Guid userId)
    {
        // Fetch the catalog items along with their related ShoppingCartItems for the user
        var catalog = await RepositoryDbContext.Items
            .Include(i => i.ShoppingCartItems!.Where(sci => sci.AppUserId == userId)) 
            .ToListAsync();

        return catalog;
    }

    // Increments its quantity, or adds an item to the user's cart if it already exists
    public async Task AddCartItem(Guid userId, Guid itemId)
    {
        var user = await RepositoryDbContext.Users
            .Include(u => u.ShoppingCartItems)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            throw new KeyNotFoundException("User not found.");

        var cartItem = user.ShoppingCartItems?.FirstOrDefault(i => i.ItemId == itemId);

        if (cartItem != null)
        {
            cartItem.Quantity++;
        }
        else
        {
            user.ShoppingCartItems ??= new List<ShoppingCartItem>();
            user.ShoppingCartItems.Add(new ShoppingCartItem
            {
                ItemId = itemId,
                AppUserId = userId,
                Quantity = 1
            });
        }

        await RepositoryDbContext.SaveChangesAsync();
    }

    // Decrements quantity of an item in the user's cart or removes it if the quantity becomes zero
    public async Task RemoveCartItem(Guid userId, Guid itemId)
    {
        var user = await RepositoryDbContext.Users
            .Include(u => u.ShoppingCartItems)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            throw new KeyNotFoundException("User not found.");

        var cartItem = user.ShoppingCartItems?.FirstOrDefault(i => i.ItemId == itemId);

        if (cartItem != null)
        {
            cartItem.Quantity--;

            if (cartItem.Quantity <= 0) RepositoryDbContext.ShoppingCartItems.Remove(cartItem);

            await RepositoryDbContext.SaveChangesAsync();
        }
    }

    // Sets the exact quantity of an item in the user's cart
    public async Task SetCartItemQuantity(Guid userId, Guid itemId, int? quantity)
    {
        if (quantity < 0)
            throw new ArgumentException("Quantity cannot be negative.", nameof(quantity));

        var user = await RepositoryDbContext.Users
            .Include(u => u.ShoppingCartItems)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            throw new KeyNotFoundException("User not found.");

        var cartItem = user.ShoppingCartItems?.FirstOrDefault(i => i.ItemId == itemId);

        if (cartItem != null)
        {
            if (quantity == 0)
            {
                RepositoryDbContext.ShoppingCartItems.Remove(cartItem);
            }
            else
            {
                cartItem.Quantity = quantity ?? 0;
            }
        }
        else if (quantity > 0)
        {
            user.ShoppingCartItems ??= new List<ShoppingCartItem>();
            user.ShoppingCartItems.Add(new ShoppingCartItem
            {
                ItemId = itemId,
                AppUserId = userId,
                Quantity = (int)quantity
            });
        }

        await RepositoryDbContext.SaveChangesAsync();
    }

    // Removes all items from the user's cart
    public async Task RemoveAllCartItems(Guid userId)
    {
        var user = await RepositoryDbContext.Users
            .Include(u => u.ShoppingCartItems)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            throw new KeyNotFoundException("User not found.");
        if (user.ShoppingCartItems?.Any() == null || !user.ShoppingCartItems.Any()) return; 

        foreach (var item in user.ShoppingCartItems)
        {
            RepositoryDbContext.ShoppingCartItems.Remove(item);
        }

        await RepositoryDbContext.SaveChangesAsync();
    }
}
