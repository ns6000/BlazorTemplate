using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using BlazorTemplate.Shared.Contracts;


namespace BlazorTemplate.Server.Services;

public class IdentityService
{
	private readonly UserManager<IdentityUser> userManager;

	public IdentityService(UserManager<IdentityUser> userManager) =>
		this.userManager = userManager;

	public async Task<List<string>> RegisterAsync(RegistrationRequest request)
	{
		try
		{
			IdentityUser existingUser = await userManager.FindByNameAsync(request.Login);
			if(existingUser is not null)
				throw new Exception($"User {request.Login} already exists");

			await userManager.CreateAsync(new IdentityUser {
				UserName	= request.Login,
				Email		= request.Email
			}, request.Password);	
		}
		catch(Exception ex)
		{
			return new List<string> {
				ex.Message
			};
		}
		
		return new List<string>();
	}

	public async Task<LoginResponse> LoginAsync(LoginRequest request)
	{
		try
		{
			IdentityUser existingUser = await userManager.FindByNameAsync(request.Login);
			if(existingUser is null)
				throw new Exception($"User '{request.Login}' does not exists");

			bool validPassword = await userManager.CheckPasswordAsync(existingUser, request.Password);
			if(!validPassword)
				throw new Exception($"Wrong password for user {request.Login}");

			JwtSecurityTokenHandler tokenHandler	= new JwtSecurityTokenHandler();
			SecurityToken token						= tokenHandler.CreateToken(new SecurityTokenDescriptor {
				Expires				= DateTime.UtcNow.AddMinutes(App.Config.JWT.ExpiresAfterMin),
				SigningCredentials	= new SigningCredentials(
					new SymmetricSecurityKey(Encoding.ASCII.GetBytes(App.Config.JWT.SecretKey)),
					SecurityAlgorithms.HmacSha256Signature
				),
				Subject				= new ClaimsIdentity(new [] {
					new Claim(JwtRegisteredClaimNames.Sub,		existingUser.UserName),
					new Claim(JwtRegisteredClaimNames.Email,	existingUser.Email),
					new Claim(JwtRegisteredClaimNames.Jti,		Guid.NewGuid().ToString())
				})
			});

			return new LoginResponse {
				IsSuccessful	= true,
				Token			= tokenHandler.WriteToken(token)
			};
		}
		catch(Exception ex)
		{
			return new LoginResponse {
				Errors = new string[] {
					ex.Message
				}
			};
		}
	}
}