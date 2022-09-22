using Microsoft.AspNetCore.Identity;


namespace BlazorTemplate.Server.Data;

public class IdentityUserEx : IdentityUser
{
	public string? RefreshToken						{ get; set; }
	public DateTime? RefreshTokenExpirationUTC		{ get; set; }
}