using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using BlazorTemplate.Server.Data;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;


namespace BlazorTemplate.Server.Services;

public record IdentityTokens(string AccessToken, string RefreshToken);

public struct JwtCustomClaimNames
{
	public const string Id = "id";
}

public class IdentityService
{
	private static readonly string signingAlgorithm						= SecurityAlgorithms.HmacSha256Signature;
	private static readonly SymmetricSecurityKey symetricSecurityKey	= new(Encoding.ASCII.GetBytes(App.Config.JWT.SecretKey));
	private readonly UserManager<IdentityUserEx> userManager;

	public static TokenValidationParameters TokenValidationParameters { get => new() {
			RequireExpirationTime		= true,
			ValidateLifetime			= true,
			ValidateAudience			= false,
			ValidateIssuer				= false,
			ValidateIssuerSigningKey	= true,
			ClockSkew					= TimeSpan.Zero,
			IssuerSigningKey			= symetricSecurityKey
		};
	}

	public IdentityService(UserManager<IdentityUserEx> userManager) =>
		this.userManager = userManager;

	private static string CreateAccessToken(IdentityUserEx user)
	{
		JwtSecurityTokenHandler tokenHandler	= new();
		SecurityToken token						= tokenHandler.CreateToken(new SecurityTokenDescriptor {
			Expires				= DateTime.UtcNow.AddSeconds(App.Config.JWT.TokenLifetimeSec),
			SigningCredentials	= new SigningCredentials(symetricSecurityKey, signingAlgorithm),
			Subject				= new ClaimsIdentity(new [] {
				new Claim(JwtCustomClaimNames.Id,			user.Id),
				new Claim(JwtRegisteredClaimNames.Sub,		user.UserName),
				new Claim(JwtRegisteredClaimNames.Email,	user.Email),
				new Claim(JwtRegisteredClaimNames.Jti,		Guid.NewGuid().ToString())
			})
		});

		return tokenHandler.WriteToken(token);
	}

	private async Task<string> CreateRefreshToken(IdentityUserEx user)
	{
		string? refreshToken = null;

		while(refreshToken is null)
		{
			refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(App.Config.JWT.RefreshTokenSize));
			if(await userManager.Users.AsNoTracking().AnyAsync(x => x.RefreshToken == refreshToken))
				refreshToken = null;
		}

		user.RefreshToken				= refreshToken;
		user.RefreshTokenExpirationUTC	= DateTime.UtcNow.AddHours(App.Config.JWT.RefreshTokenLifetimeHours);
		await userManager.UpdateAsync(user);

		return refreshToken;
	}

	private async Task RevokeRefreshToken(IdentityUserEx user)
	{
		user.RefreshToken				= null;
		user.RefreshTokenExpirationUTC	= null;
		await userManager.UpdateAsync(user);
	}

	public async Task RegisterAsync(string login, string password, string? email)
	{
		ArgumentNullException.ThrowIfNull(login);
		ArgumentNullException.ThrowIfNull(password);

		IdentityUserEx user = await userManager.FindByNameAsync(login);
		if(user is not null)
			throw new ArgumentException($"User '{login}' already exists");

		await userManager.CreateAsync(new IdentityUserEx {
			UserName	= login,
			Email		= string.IsNullOrEmpty(email) ? null : email.Trim()
		}, password);
	}

	public async Task<IdentityTokens> LoginAsync(string login, string password)
	{
		ArgumentNullException.ThrowIfNull(login);
		ArgumentNullException.ThrowIfNull(password);

		IdentityUserEx user = await userManager.FindByNameAsync(login);
		if(user is null)
			throw new UnauthorizedAccessException($"User '{login}' does not exists");

		bool validPassword = await userManager.CheckPasswordAsync(user, password);
		if(!validPassword)
			throw new UnauthorizedAccessException($"Wrong password for user '{login}'");

		return new IdentityTokens(
			CreateAccessToken(user),
			await CreateRefreshToken(user)
		);
	}

	public async Task<IdentityTokens> RefreshTokenAsync(string refreshToken)
	{
		ArgumentNullException.ThrowIfNull(refreshToken);

		IdentityUserEx? user = await userManager
			.Users
			.FirstOrDefaultAsync(x => x.RefreshToken == refreshToken);

		if(user is null)
			throw new UnauthorizedAccessException("Invalid token");

		if(!user.RefreshTokenExpirationUTC.HasValue || user.RefreshTokenExpirationUTC.Value < DateTime.UtcNow)
		{
			await RevokeRefreshToken(user);
			throw new UnauthorizedAccessException("Expired token");
		}

		return new IdentityTokens(
			CreateAccessToken(user),
			await CreateRefreshToken(user)
		);
	}

	public async Task RevokeRefreshTokenAsync(string refreshToken)
	{
		IdentityUserEx? user = await userManager
			.Users
			.FirstOrDefaultAsync(x => x.RefreshToken == refreshToken);

		if(user is null)
			throw new UnauthorizedAccessException("Invalid token");

		await RevokeRefreshToken(user);
	}
}