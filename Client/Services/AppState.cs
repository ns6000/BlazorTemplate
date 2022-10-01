using System.Reflection;
using System.IdentityModel.Tokens.Jwt;


namespace BlazorTemplate.Client.Services;

public class AppState
{
	public event Action? OnChange;

	private JwtSecurityToken? accessToken = default;
	public JwtSecurityToken? AccessToken
	{
		get => accessToken;
		set {
			accessToken = value;
			OnChange?.Invoke();
		}
	}

	private uint counter = default;
	public uint Counter
	{
		get => counter;
		set {
			counter = value;
			OnChange?.Invoke();
		}
	}

	public void ClearState() =>
		this
		.GetType()
		.GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
		.ToList()
		.ForEach(x => x.SetValue(this, default));
}