using App.BLL.Mappers;
using App.BLL.Services;
using App.DAL.Contracts;
using App.DAL.Seeding;
using App.Domain.Entities;
using App.Domain.Identity;
using AutoMapper;
using Moq;
using Public.DTO.v1.ShoppingCartItems;

namespace TestProject1.Unit;

public class LessonsServiceUnitTests
{
    private readonly Mock<IAppUOW> _uowMock;
    private readonly Mock<ShopItemDetailsMapper> _detailsMapperMock;
    private readonly Mock<ShopItemListElemMapper> _listMapperMock;
    private readonly ShopItemService _shopItemService;
    private readonly Mock<IMapper> _mapperMock;

    public LessonsServiceUnitTests()
    {
        // Initialize the mocks
        _uowMock = new Mock<IAppUOW>();
        _mapperMock = new Mock<IMapper>();
        _detailsMapperMock = new Mock<ShopItemDetailsMapper>(_mapperMock.Object);
        _listMapperMock = new Mock<ShopItemListElemMapper>(_mapperMock.Object);
        _shopItemService = new ShopItemService(_uowMock.Object, _detailsMapperMock.Object, _listMapperMock.Object);
        new DataGuids();
    }

    [Fact]
    public async Task AddRemoveCartItem_ShouldIncrementItemQuantity_WhenActionIsIncrement()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var itemId = Guid.NewGuid();

        var user = new AppUser
        {
            Id = userId,
            ShoppingCartItems = new List<ShoppingCartItem>
            {
                new ShoppingCartItem
                {
                    ItemId = itemId,
                    AppUserId = userId,
                    Quantity = 1 // Starting with a quantity of 1
                }
            }
        };

        // Mock the repository to return the user with the shopping cart item
        _uowMock.Setup(u => u.ShopItemRepository.AddCartItem(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.CompletedTask);
        _uowMock.Setup(u => u.ShopItemRepository.RemoveCartItem(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.CompletedTask);
        _uowMock.Setup(u => u.ShopItemRepository.SetCartItemQuantity(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int?>())).Returns(Task.CompletedTask);

        // Act
        await _shopItemService.AddRemoveCartItem(userId, itemId, ECartItemActions.Increment, null);

        // Assert
        _uowMock.Verify(u => u.ShopItemRepository.AddCartItem(It.Is<Guid>(id => id == userId), It.Is<Guid>(id => id == itemId)), Times.Once);
    }

    [Fact]
    public async Task AddRemoveCartItem_ShouldDecrementItemQuantity_WhenActionIsDecrement()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var itemId = Guid.NewGuid();

        var user = new AppUser
        {
            Id = userId,
            ShoppingCartItems = new List<ShoppingCartItem>
            {
                new()
                {
                    ItemId = itemId,
                    AppUserId = userId,
                    Quantity = 2 // Starting with a quantity greater than 1
                }
            }
        };

        // Mock the repository to return the user with the shopping cart item
        _uowMock.Setup(u => u.ShopItemRepository.AddCartItem(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.CompletedTask);
        _uowMock.Setup(u => u.ShopItemRepository.RemoveCartItem(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.CompletedTask);
        _uowMock.Setup(u => u.ShopItemRepository.SetCartItemQuantity(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int?>())).Returns(Task.CompletedTask);

        // Act
        await _shopItemService.AddRemoveCartItem(userId, itemId, ECartItemActions.Decrement, null);

        // Assert
        _uowMock.Verify(u => u.ShopItemRepository.RemoveCartItem(It.Is<Guid>(id => id == userId), It.Is<Guid>(id => id == itemId)), Times.Once);
    }

    [Fact]
    public async Task AddRemoveCartItem_ShouldSetItemQuantity_WhenActionIsSetAmount()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var quantity = 10;

        var user = new AppUser
        {
            Id = userId,
            ShoppingCartItems = new List<ShoppingCartItem>
            {
                new ShoppingCartItem
                {
                    ItemId = itemId,
                    AppUserId = userId,
                    Quantity = 1 // Initial quantity
                }
            }
        };

        // Mock repository behavior
        _uowMock.Setup(u => u.ShopItemRepository.SetCartItemQuantity(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int?>())).Returns(Task.CompletedTask);
        _uowMock.Setup(u => u.ShopItemRepository.GetCartItems(userId))
            .ReturnsAsync(user.ShoppingCartItems);

        // Act
        await _shopItemService.AddRemoveCartItem(userId, itemId, ECartItemActions.SetAmount, quantity);

        // Assert
        _uowMock.Verify(u => u.ShopItemRepository.SetCartItemQuantity(It.Is<Guid>(id => id == userId), It.Is<Guid>(id => id == itemId), It.Is<int?>(q => q == quantity)), Times.Once);
    }

    [Fact]
    public async Task RemoveAllCartItems_ShouldRemoveAllItemsFromCart()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Mock the repository behavior
        _uowMock.Setup(u => u.ShopItemRepository.RemoveAllCartItems(It.IsAny<Guid>())).Returns(Task.CompletedTask);

        // Act
        await _shopItemService.RemoveAllCartItems(userId);

        // Assert
        _uowMock.Verify(u => u.ShopItemRepository.RemoveAllCartItems(It.Is<Guid>(id => id == userId)), Times.Once);
    }
}
