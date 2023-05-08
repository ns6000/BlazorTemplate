using Microsoft.AspNetCore.Http;
using BlazorTemplate.Shared.Contracts;
using BlazorTemplate.Server.Services;


namespace BlazorTemplate.Server.Controllers;

[ApiController]
public class IdentityController : ControllerBase
{
	private const string refreshTokenCookieName = "refreshToken";
	private readonly IdentityService identityService;

	public IdentityController(IdentityService identityService) =>
		this.identityService = identityService;

	private readonly CookieOptions refreshTokenCookieOptions = new() {
		HttpOnly	= true,
		Secure		= !Constants.DEBUG,
		IsEssential = true,
		SameSite	= SameSiteMode.Strict,
		Domain		= Constants.DEBUG ? null : App.Config.JWT.Domain,
		MaxAge		= TimeSpan.FromHours(App.Config.JWT.RefreshTokenLifetimeHours)
	};

#if !DEBUG
	[Authorize]
#endif
	[HttpPost(ApiRoutes.Identity.Register)]
	public async Task<IActionResult> Register([FromBody]RegistrationRequest request)
	{
		await identityService.RegisterAsync(request.Login!, request.Password!, request.Email, request.Role!);
		return Ok();
	}

	[HttpPost(ApiRoutes.Identity.Login)]
	public async Task<IActionResult> Login([FromBody]LoginRequest request)
	{
		IdentityTokens identityTokens = await identityService.LoginAsync(request.Login!, request.Password!);
		Response.Cookies.Append(refreshTokenCookieName, identityTokens.RefreshToken, refreshTokenCookieOptions);
		return Ok(identityTokens.AccessToken);
	}

	[HttpPost(ApiRoutes.Identity.Logout)]
	public async Task<IActionResult> Logout()
	{
		if(Request.Cookies[refreshTokenCookieName] is not string refreshToken)
			throw new UnauthorizedAccessException();

		await identityService.RevokeRefreshTokenAsync(refreshToken);
		Response.Cookies.Delete(refreshTokenCookieName);
		return Ok();
	}

	[HttpPost(ApiRoutes.Identity.Refresh)]
	public async Task<IActionResult> Refresh()
	{
		if(Request.Cookies[refreshTokenCookieName] is not string refreshToken)
			throw new UnauthorizedAccessException();

		IdentityTokens identityTokens = await identityService.RefreshTokenAsync(refreshToken);
		Response.Cookies.Append(refreshTokenCookieName, identityTokens.RefreshToken, refreshTokenCookieOptions);
		return Ok(identityTokens.AccessToken);
	}
}