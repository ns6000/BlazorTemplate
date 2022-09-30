using System.IdentityModel.Tokens.Jwt;


namespace BlazorTemplate.Client.Services;

public class AppState
{
	public event Action? OnChange;

	private JwtSecurityToken? accessToken = null;

	public JwtSecurityToken? AccessToken
	{
		get => accessToken;
		set {
			accessToken = value;
			OnChange?.Invoke();
		}
	}
}