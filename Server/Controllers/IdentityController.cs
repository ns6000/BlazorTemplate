using BlazorTemplate.Shared;
using BlazorTemplate.Shared.Routes;
using BlazorTemplate.Shared.Contracts;
using BlazorTemplate.Server.Services;
using Microsoft.AspNetCore.Http;

namespace BlazorTemplate.Server.Controllers;

[ApiController]
public class IdentityController : ControllerBase
{
	public const string refreshTokenCookieName = "refreshToken";
	public readonly IdentityService identityService;

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
	[HttpPost(Routes.Identity.Register)]
	public async Task<IActionResult> Register([FromBody]RegistrationRequest request)
	{
		await identityService.RegisterAsync(request.Login!, request.Password!, request.Email);
		return Ok();
	}

	[HttpPost(Routes.Identity.Login)]
	public async Task<IActionResult> Login([FromBody]LoginRequest request)
	{
		IdentityTokens identityTokens = await identityService.LoginAsync(request.Login!, request.Password!);
		Response.Cookies.Append(refreshTokenCookieName, identityTokens.RefreshToken, refreshTokenCookieOptions);
		return Ok(identityTokens.AccessToken);
	}

	[HttpPost(Routes.Identity.Logout)]
	public async Task<IActionResult> Logout()
	{
		string? refreshToken = Request.Cookies[refreshTokenCookieName];
		if(refreshToken is null)
			throw new UnauthorizedAccessException();

		await identityService.RevokeRefreshTokenAsync(refreshToken);
		Response.Cookies.Delete(refreshTokenCookieName);
		return Ok();
	}

	[HttpPost(Routes.Identity.Refresh)]
	public async Task<IActionResult> Refresh()
	{
		string? refreshToken = Request.Cookies[refreshTokenCookieName];
		if(refreshToken is null)
			throw new UnauthorizedAccessException();

		IdentityTokens identityTokens = await identityService.RefreshTokenAsync(refreshToken);
		Response.Cookies.Append(refreshTokenCookieName, identityTokens.RefreshToken, refreshTokenCookieOptions);
		return Ok(identityTokens.AccessToken);
	}
}