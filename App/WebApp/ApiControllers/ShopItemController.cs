using System.Net;
using System.Net.Mime;
using App.BLL.Contracts;
using Asp.Versioning;
using Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Public.DTO.v1.ApiResponses;
using Public.DTO.v1.ShoppingCartItems;

namespace WebApp.ApiControllers;

/// <summary>
/// API controller for handling requests related to shop and cart items.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
public class ShopItemController : ControllerBase
{
    private readonly IAppBLL _bll;
    private readonly ILogger<ShopItemController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ShopItemController"/> class.
    /// </summary>
    /// <param name="bll">The business logic layer instance.</param>
    /// <param name="logger">The logger instance.</param>
    public ShopItemController(IAppBLL bll, ILogger<ShopItemController> logger)
    {
        _bll = bll;
        _logger = logger;
    }

    /// <summary>
    /// Get a list of all items in the shopping cart.
    /// </summary>
    /// <returns>A list of items.</returns>
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<ShopItemListElement>), StatusCodes.Status200OK)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<IEnumerable<ShopItemListElement?>>> GetAllCartItems()
    {
        Guid userId = User.GetUserId();
        _logger.LogInformation("Fetching all cart items for user {UserId}", userId);

        try
        {
            var cartItems = await _bll.ShopItemService.GetCartItems(userId);
            _logger.LogInformation("Successfully retrieved {Count} cart items for user {UserId}", cartItems.Count(), userId);
            return Ok(cartItems);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error retrieving the cart items list for user {UserId}", userId);
            return FormatErrorResponse($"Error retrieving the cart items list: {e.Message}");
        }
    }

    /// <summary>
    /// Get the shop item details.
    /// </summary>
    /// <param name="itemId">The ID of the item.</param>
    /// <returns>The details of the shop item.</returns>
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ShopItemDetails), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "Optional")]
    public async Task<IActionResult> GetShopItemDetails([FromQuery] string itemId)
    {
        // Try to get the user ID, if available (optional authorization)
        Guid? userId = null;
        try
        {
            userId = User.GetUserId();
        }
        catch (Exception)
        {
            // No user is attached, proceed as unauthenticated
        }

        string userIdForLogging = userId == null ? "Guest" : userId.ToString()!;
        _logger.LogInformation("Fetching details for cart item {ItemId} for user {UserId}", itemId, userIdForLogging);

        try
        {
            var item = await _bll.ShopItemService.GetShopItem(userId, Guid.Parse(itemId));
            if (item != null)
            {
                _logger.LogInformation("Successfully retrieved details for cart item {ItemId} for user {UserId}", itemId, userIdForLogging);
                return Ok(item);
            }
            _logger.LogWarning("Cart item {ItemId} not found for user {UserId}", itemId, userIdForLogging);
            return FormatErrorResponse("Error finding the cart item:");
        }
        catch (KeyNotFoundException e)
        {
            _logger.LogError(e, "Could not find an item with id: {ItemId}", itemId);
            return FormatErrorResponse($"Error getting the item details: {e.Message}");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting the item details for cart item {ItemId} for user {UserId}", itemId, userIdForLogging);
            return FormatErrorResponse($"Error getting the item details: {e.Message}");
        }
    }

    /// <summary>
    /// Endpoint for adding or removing items from the shopping cart.
    /// </summary>
    /// <param name="shoppingCartItemAction">The action of the user.</param>
    /// <returns></returns>
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpPut]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> AddRemoveCartItem([FromBody] ShoppingCartItemAction shoppingCartItemAction)
    {
        Guid userId = User.GetUserId();
        _logger.LogInformation("User {UserId} requested {Action} for item {ItemId}", userId, shoppingCartItemAction.ItemAction, shoppingCartItemAction.ItemId);

        try
        {
            await _bll.ShopItemService.AddRemoveCartItem(userId, shoppingCartItemAction.ItemId, shoppingCartItemAction.ItemAction, shoppingCartItemAction.Quantity);
            _logger.LogInformation("Successfully performed {Action} for item {ItemId} for user {UserId}", shoppingCartItemAction.ItemAction, shoppingCartItemAction.ItemId, userId);
            return Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error performing {Action} for item {ItemId} for user {UserId}", shoppingCartItemAction.ItemAction, shoppingCartItemAction.ItemId, userId);
            return FormatErrorResponse($"Error adding/removing the cart item: {e.Message}");
        }
    }

    /// <summary>
    /// Endpoint for clearing the shopping cart.
    /// </summary>
    /// <returns></returns>
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpDelete]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> ClearShoppingCart()
    {
        Guid userId = User.GetUserId();
        _logger.LogInformation("User {UserId} requested clearing the shopping cart", userId);

        try
        {
            await _bll.ShopItemService.RemoveAllCartItems(userId);
            _logger.LogInformation("Successfully cleared the shopping cart for user {UserId}", userId);
            return Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred when clearing the shopping cart for user {UserId}", userId);
            return FormatErrorResponse($"Error occurred when clearing the shopping cart: {e.Message}");
        }
    }

    /// <summary>
    /// Get a list of all items in the shop.
    /// </summary>
    /// <returns>A list of all shop items.</returns>
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<ShopItemListElement>), StatusCodes.Status200OK)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "Optional")]
    public async Task<ActionResult<IEnumerable<ShopItemListElement?>>> GetAllShopItems()
    {
        _logger.LogInformation("Fetching all shop items");

        // Try to get the user ID, if available (optional authorization)
        Guid? userId = null;
        try
        {
            userId = User.GetUserId();
        }
        catch (Exception)
        {
            // No user is attached, proceed as unauthenticated
        }

        try
        {
            // Fetch all items, with or without quantity depending on user authentication
            var shopItems = await _bll.ShopItemService.AllAsync(userId);
            _logger.LogInformation("Successfully retrieved {Count} shop items", shopItems.Count());
            return Ok(shopItems);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error retrieving the shop items list");
            return FormatErrorResponse($"Error retrieving the shop items list: {e.Message}");
        }
    }

    private ActionResult FormatErrorResponse(string message)
    {
        return BadRequest(new RestApiErrorResponse
        {
            Status = HttpStatusCode.BadRequest,
            Error = message
        });
    }
}