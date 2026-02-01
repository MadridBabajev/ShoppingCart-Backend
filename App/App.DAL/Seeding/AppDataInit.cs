using App.Domain.Entities;
using App.Domain.Identity;
using Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace App.DAL.Seeding;

public static class AppDataInit
{
    private static readonly string DescriptionFiller =
        "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Curabitur ornare mauris eu magna mollis, quis blandit ex sagittis. Pellentesque pulvinar, ligula feugiat finibus venenatis, lectus erat hendrerit dui, et malesuada nunc orci mollis ipsum. Donec egestas justo sit amet iaculis dapibus. Etiam arcu massa, hendrerit eget sapien fermentum, eleifend suscipit dolor. Duis egestas nulla lobortis lectus blandit fringilla. Maecenas felis mi, accumsan id felis eu, tincidunt gravida lorem.";

    private static readonly string SneakersImgPath = "sneakers.jpg";
    private static readonly string TShirtImgPath = "t_shirt.jpg";
    private static readonly string LaptopImgPath = "laptop.jpg";
    private static readonly string CapImgPath = "cap.jpg";
    private static readonly string BagImgPath = "bag.jpg";
    public static void MigrateDatabase(ApplicationDbContext context)
        => context.Database.Migrate();

    public static void DropDatabase(ApplicationDbContext context)
        => context.Database.EnsureDeleted();
     
    public static void ClearData(ApplicationDbContext context)
    {
        foreach (var tableName in context.Model.GetEntityTypes().Select(t => t.GetTableName()).Distinct())
        {
            context.Database.ExecuteSql($"TRUNCATE TABLE  \"{tableName}\" CASCADE");
        }
        context.SaveChanges();
    }
    
    public static void SeedIdentity(UserManager<AppUser> userManager, ApplicationDbContext ctx, DataGuids guids)
    {
        var user1Data = GetUser1Data(guids);
        var user2Data = GetUser2Data(guids);
        
        CreateUser(user1Data, userManager, ctx);
        CreateUser(user2Data, userManager, ctx);
    }

    private static void CreateUser(
        (Guid Id, string email, string pwd, string firstName, string lastName) userData,
        UserManager<AppUser> userManager, ApplicationDbContext ctx)
    {
        
        AppUser? user = userManager.FindByEmailAsync(userData.email).Result;
        if (user == null)
        {
            user = GetUserWithAppliedData(userData);
            var result = userManager.CreateAsync(user, userData.pwd).Result;
            if (!result.Succeeded)
            {
                throw new ApplicationException($"Cannot seed users, {result}");
            }
            
            ctx.SaveChanges();
        }
    }

    private static AppUser GetUserWithAppliedData((Guid id, string email, string pwd, string firstName, string lastName) userData)
        => new() 
        {
            Id = userData.id,
            Email = userData.email,
            FirstName = userData.firstName,
            LastName = userData.lastName,
            UserName = userData.email,
            EmailConfirmed = true,
        };

    private static (Guid Id, string email, string pwd, string firstName, string lastName)
        GetUser1Data(DataGuids dataGuids)
        => (dataGuids.User1Id, "testUser1@app.com", "Foo.bar.user1", "User1", "Test");
    
    private static (Guid Id, string email, string pwd, string firstName, string lastName)
        GetUser2Data(DataGuids dataGuids)
        => (dataGuids.User2Id, "testUser2@app.com", "Foo.bar.user2", "User2", "Test");
    
    public static void SeedAppData(ApplicationDbContext context, string basePath, DataGuids guids)
    {
        SeedItems(context, basePath, guids);
        context.SaveChanges();
    }

    private static void SeedItems(ApplicationDbContext context, string basePath, DataGuids guids)
    {
        if (context.Items.Any()) return;
        
        context.Items.AddRange(new ShopItem
        {
            Id = guids.Item1Id,
            Name = "Sneakers",
            Description = DescriptionFiller,
            Rating = 4.5,
            StockAmount = 10,
            Price = 50,
            ItemPicture = FileOperationsHelpers.LoadImageAsBytes(Path.Combine(basePath, SneakersImgPath))
        }, new ShopItem
        {
            Id = guids.Item2Id,
            Name = "T-shirts",
            Description = DescriptionFiller,
            Rating = 4.4,
            StockAmount = 20,
            Price = 15,
            ItemPicture = FileOperationsHelpers.LoadImageAsBytes(Path.Combine(basePath, TShirtImgPath))
        }, new ShopItem
        {
            Id = guids.Item3Id,
            Name = "Laptops",
            Description = DescriptionFiller,
            Rating = 5,
            StockAmount = 1,
            Price = 1600,
            ItemPicture = FileOperationsHelpers.LoadImageAsBytes(Path.Combine(basePath, LaptopImgPath))
        }, new ShopItem
        {
            Id = guids.Item4Id,
            Name = "Caps",
            Description = DescriptionFiller,
            Rating = 5,
            StockAmount = 50,
            Price = 15,
            ItemPicture = FileOperationsHelpers.LoadImageAsBytes(Path.Combine(basePath, CapImgPath))
        }, new ShopItem
        {
            Id = guids.Item5Id,
            Name = "Bags",
            Description = DescriptionFiller,
            Rating = 4.7,
            StockAmount = 25,
            Price = 60,
            ItemPicture = FileOperationsHelpers.LoadImageAsBytes(Path.Combine(basePath, BagImgPath))
        });
        context.SaveChanges();
    }
}