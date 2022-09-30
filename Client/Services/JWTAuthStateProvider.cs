using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;


namespace BlazorTemplate.Client.Services;

public class JWTAuthStateProvider : AuthenticationStateProvider
{
	private readonly AppState appState;

	public JWTAuthStateProvider(AppState appState) =>
		this.appState = appState;

	public Task<AuthenticationState> CreateAuthStateAsync() =>
		Task.FromResult(
			new AuthenticationState(new ClaimsPrincipal(appState.AccessToken is not null
				? new ClaimsIdentity(appState.AccessToken.Claims, "JWT")
				: new ClaimsIdentity()
			))
		);

	public override Task<AuthenticationState> GetAuthenticationStateAsync() =>
		CreateAuthStateAsync();

	public void NotifyUserAuthentication() =>
		NotifyAuthenticationStateChanged(CreateAuthStateAsync());
}