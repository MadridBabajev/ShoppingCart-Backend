using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mime;
using System.Security.Claims;
using System.Text;
using App.DAL;
using App.Domain.Identity;
using Asp.Versioning;
using Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Public.DTO.v1.ApiResponses;
using Public.DTO.v1.Identity;

namespace WebApp.ApiControllers.Identity;

/// <summary>
/// Controller responsible for account related actions.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/identity/[controller]/[action]")]
public class AccountController : ControllerBase
{
    private readonly SignInManager<AppUser> _signInManager;
    private readonly UserManager<AppUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AccountController> _logger;
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of the AccountController class.
    /// </summary>
    public AccountController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager,
        IConfiguration configuration, ILogger<AccountController> logger, ApplicationDbContext context)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _configuration = configuration;
        _logger = logger;
        _context = context;
    }

    /// <summary>
    /// Registers a new user in the system.
    /// </summary>
    /// <param name="registrationData">User registration details.</param>
    /// <param name="expiresInSeconds">Optional expiration time for JWT in seconds.</param>
    /// <returns>A JWT token and refresh token if registration is successful.</returns>
    [HttpPost]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(JWTResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RestApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<JWTResponse>> Register([FromBody] Register registrationData,
        [FromQuery] int? expiresInSeconds)
    {
        // Set default JWT expiration if not provided
        expiresInSeconds ??= _configuration.GetValue<int>("JWT:ExpiresInSeconds");

        // Check if the user is already registered
        var existingUsers = await _userManager.Users.Where(u => u.Email == registrationData.Email).ToListAsync();
        if (existingUsers.Any())
        {
            _logger.LogWarning("User with email {} is already registered", registrationData.Email);
            return FormatErrorResponse($"User with email {registrationData.Email} is already registered");
        }

        // Validate password confirmation
        if (!registrationData.Password.Equals(registrationData.ConfirmPassword))
        {
            _logger.LogWarning("Password and confirm password do not match");
            return FormatErrorResponse("Password and Confirmation password do not match");
        }

        // Create new user and refresh token
        var refreshToken = new AppRefreshToken();
        var appUser = new AppUser
        {
            Email = registrationData.Email,
            UserName = registrationData.Email,
            FirstName = registrationData.Firstname,
            LastName = registrationData.Lastname,
            AppRefreshTokens = new List<AppRefreshToken> {refreshToken},
        };
        refreshToken.AppUser = appUser;

        // Register user with the provided password
        var result = await _userManager.CreateAsync(appUser, registrationData.Password);
        if (!result.Succeeded)
        {
            _logger.LogError("Error registering user: {ErrorDescription}", result.Errors.First().Description);
            return FormatErrorResponse(result.Errors.First().Description);
        }

        await _context.SaveChangesAsync();

        // Add claims for the new user
        result = await _userManager.AddClaimsAsync(appUser, new List<Claim>
        {
            new(ClaimTypes.GivenName, appUser.FirstName),
            new(ClaimTypes.Surname, appUser.LastName)
        });
        if (!result.Succeeded)
        {
            return FormatErrorResponse(result.Errors.First().Description);
        }

        // Find the newly registered user
        appUser = await _userManager.FindByEmailAsync(appUser.Email);
        if (appUser == null)
        {
            _logger.LogWarning("User with email {} not found after registration", registrationData.Email);
            return BadRequest(new RestApiErrorResponse
            {
                Status = HttpStatusCode.BadRequest,
                Error = $"User with email {registrationData.Email} not found after registration"
            });
        }

        // Create claims principal and generate JWT
        var claimsPrincipal = await _signInManager.CreateUserPrincipalAsync(appUser);
        var jwt = GenerateJwt(claimsPrincipal.Claims, (int)expiresInSeconds);

        var res = new JWTResponse
        {
            JWT = jwt,
            RefreshToken = refreshToken.RefreshToken,
            ExpiresIn = (int)expiresInSeconds
        };

        return Ok(res);
    }

    /// <summary>
    /// Logs in a user by verifying their credentials.
    /// </summary>
    /// <param name="loginData">User login information (email and password).</param>
    /// <param name="expiresInSeconds">Optional expiration time for JWT in seconds.</param>
    /// <returns>A JWT token and refresh token if login is successful.</returns>
    [HttpPost]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(JWTResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RestApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<JWTResponse>> LogIn([FromBody] Login loginData, [FromQuery] int? expiresInSeconds)
    {
        expiresInSeconds ??= _configuration.GetValue<int>("JWT:ExpiresInSeconds");

        // Find the user by email
        var appUser = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == loginData.Email);
        if (appUser == null)
        {
            _logger.LogWarning("Login failed, email {} not found", loginData.Email);
            return FormatErrorResponse("No user with the provided email was found");
        }

        // Verify password
        var result = await _signInManager.CheckPasswordSignInAsync(appUser, loginData.Password, false);
        if (!result.Succeeded)
        {
            _logger.LogWarning("Login failed, incorrect password for user {}", loginData.Email);
            return FormatErrorResponse("User/Password problem");
        }

        // Create claims principal and generate JWT
        var claimsPrincipal = await _signInManager.CreateUserPrincipalAsync(appUser);
        var jwt = GenerateJwt(claimsPrincipal.Claims, (int)expiresInSeconds);

        // Add a new refresh token and remove expired ones
        appUser.AppRefreshTokens = await _context.Entry(appUser)
            .Collection(a => a.AppRefreshTokens!)
            .Query()
            .Where(t => t.AppUserId == appUser.Id)
            .ToListAsync();

        foreach (var userRefreshToken in appUser.AppRefreshTokens)
        {
            if (userRefreshToken.ExpirationDT < DateTime.UtcNow && (userRefreshToken.PreviousExpirationDT == null ||
                                                                    userRefreshToken.PreviousExpirationDT <
                                                                    DateTime.UtcNow))
            {
                _context.AppRefreshTokens.Remove(userRefreshToken);
            }
        }

        var refreshToken = new AppRefreshToken
        {
            AppUserId = appUser.Id
        };
        _context.AppRefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        var res = new JWTResponse
        {
            JWT = jwt,
            RefreshToken = refreshToken.RefreshToken,
            ExpiresIn = (int)expiresInSeconds
        };

        return Ok(res);
    }

    /// <summary>
    /// Refreshes the JWT using a valid refresh token.
    /// </summary>
    /// <param name="refreshTokenModel">Refresh token model containing JWT and refresh token.</param>
    /// <param name="expiresInSeconds">Optional new JWT expiration time.</param>
    /// <returns>New JWT and refresh token, or error message.</returns>
    [HttpPost]
    public async Task<ActionResult> RefreshToken([FromBody] RefreshTokenModel refreshTokenModel, [FromQuery] int? expiresInSeconds)
    {
        expiresInSeconds ??= _configuration.GetValue<int>("JWT:ExpiresInSeconds");

        // Validate and read the provided JWT
        JwtSecurityToken jwtToken;
        try
        {
            jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(refreshTokenModel.Jwt);
            if (jwtToken == null)
            {
                return FormatErrorResponse("No token");
            }
        }
        catch (Exception e)
        {
            return FormatErrorResponse($"Cannot parse the token: {e.Message}");
        }

        // Validate token against secret key, issuer, and audience
        if (!IdentityHelpers.ValidateToken(refreshTokenModel.Jwt, _configuration["JWT:Key"]!,
            _configuration["JWT:Issuer"]!, _configuration["JWT:Audience"]!))
        {
            return FormatErrorResponse("JWT validation failed");
        }

        // Extract user email from JWT and fetch the user
        var userEmail = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
        if (userEmail == null)
        {
            return FormatErrorResponse("No email in JWT");
        }

        var appUser = await _userManager.Users
            .Include(u => u.AppRefreshTokens)
            .FirstOrDefaultAsync(u => u.Email == userEmail);
        if (appUser == null)
        {
            return FormatErrorResponse($"User with email {userEmail} not found");
        }

        // Load and validate refresh tokens
        await _context.Entry(appUser)
            .Collection(u => u.AppRefreshTokens!)
            .Query()
            .Where(x =>
                (x.RefreshToken == refreshTokenModel.RefreshToken && x.ExpirationDT > DateTime.UtcNow) ||
                (x.PreviousRefreshToken == refreshTokenModel.RefreshToken &&
                 x.PreviousExpirationDT > DateTime.UtcNow))
            .ToListAsync();

        if (appUser.AppRefreshTokens == null || appUser.AppRefreshTokens.Count == 0)
        {
            return FormatErrorResponse("No valid refresh tokens found");
        }

        // Generate new JWT and update refresh token
        var claimsPrincipal = await _signInManager.CreateUserPrincipalAsync(appUser);
        var jwt = GenerateJwt(claimsPrincipal.Claims, (int)expiresInSeconds);

        var refreshToken = appUser.AppRefreshTokens.First();
        if (refreshToken.RefreshToken == refreshTokenModel.RefreshToken)
        {
            refreshToken.PreviousRefreshToken = refreshToken.RefreshToken;
            refreshToken.PreviousExpirationDT = DateTime.UtcNow.AddMinutes(1);
            refreshToken.RefreshToken = Guid.NewGuid().ToString();
            refreshToken.ExpirationDT = DateTime.UtcNow.AddDays(14);

            await _context.SaveChangesAsync();
        }

        var res = new JWTResponse
        {
            JWT = jwt,
            RefreshToken = refreshToken.RefreshToken,
            ExpiresIn = (int)expiresInSeconds
        };

        return Ok(res);
    }

    /// <summary>
    /// Logs out a user by invalidating their refresh token.
    /// </summary>
    /// <param name="logout">Logout request containing the refresh token.</param>
    /// <returns>OK result if logout is successful, or an error message.</returns>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpPost]
    public async Task<ActionResult> Logout([FromBody] Logout logout)
    {
        // Get the current user's ID
        var userId = User.GetUserId();

        // Find the user by ID
        var appUser = await _context.Users.SingleOrDefaultAsync(u => u.Id == userId);
        if (appUser == null)
        {
            return FormatErrorResponse("No user was found");
        }

        // Find and remove the refresh tokens
        await _context.Entry(appUser)
            .Collection(u => u.AppRefreshTokens!)
            .Query()
            .Where(x =>
                x.RefreshToken == logout.RefreshToken || x.PreviousRefreshToken == logout.RefreshToken)
            .ToListAsync();

        foreach (var appRefreshToken in appUser.AppRefreshTokens!)
        {
            _context.AppRefreshTokens.Remove(appRefreshToken);
        }

        var deleteCount = await _context.SaveChangesAsync();

        return Ok(new { TokenDeleteCount = deleteCount });
    }

    /// <summary>
    /// Generates a JWT token with claims and expiration time.
    /// </summary>
    /// <param name="claims">Claims to include in the JWT.</param>
    /// <param name="expiresInSeconds">Expiration time in seconds.</param>
    /// <returns>The generated JWT as a string.</returns>
    private string GenerateJwt(IEnumerable<Claim> claims, int expiresInSeconds)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.Now.AddSeconds(expiresInSeconds);

        var token = new JwtSecurityToken(
            issuer: _configuration["JWT:Issuer"],
            audience: _configuration["JWT:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Formats an error response with a message.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <returns>BadRequest with an error response.</returns>
    private ActionResult FormatErrorResponse(string message)
    {
        return BadRequest(new RestApiErrorResponse
        {
            Status = HttpStatusCode.BadRequest,
            Error = message
        });
    }
}
