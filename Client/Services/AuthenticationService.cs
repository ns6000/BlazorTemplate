using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Authorization;
using BlazorTemplate.Shared;
using BlazorTemplate.Shared.Contracts;


namespace BlazorTemplate.Client.Services;

public class AuthenticationService
{
	private readonly HttpClient httpClient;
	private readonly AppState appState;
	private readonly AuthenticationStateProvider authStateProvider;

	public AuthenticationService(HttpClient httpClient, AppState appState, AuthenticationStateProvider authStateProvider)
	{
		this.httpClient			= httpClient;
		this.appState			= appState;
		this.authStateProvider	= authStateProvider;
	}

	private async Task ProcessResponse(HttpResponseMessage response)
	{
		string accessToken		= await response.Content.ReadAsStringAsync();
		appState.AccessToken	= new JwtSecurityTokenHandler().ReadJwtToken(accessToken);

		((JWTAuthStateProvider)authStateProvider).NotifyUserAuthentication();
	}

	public async Task<bool> Login(LoginRequest loginRequest)
	{
		try
		{
			HttpResponseMessage response = await httpClient.PostAsJsonAsync(ApiRoutes.Identity.Login, loginRequest);
			if(!response.IsSuccessStatusCode)
				throw new Exception($"Login failed with {(int)response.StatusCode} ({response.StatusCode})");

			await ProcessResponse(response);

			return true;
		}
		catch(Exception)
		{
			return false;
		}
	}

	public async Task Logout()
	{
		try
		{
			await httpClient.PostAsync(ApiRoutes.Identity.Logout, null);
			appState.ClearState();

			((JWTAuthStateProvider)authStateProvider).NotifyUserAuthentication();
		}
		catch(Exception)
		{
		}
	}

	public async Task Refresh()
	{
		try
		{
			HttpResponseMessage response = await httpClient.PostAsync(ApiRoutes.Identity.Refresh, null);
			if(!response.IsSuccessStatusCode)
				throw new Exception($"Refresh failed with {(int)response.StatusCode} ({response.StatusCode})");

			await ProcessResponse(response);
		}
		catch(Exception)
		{
		}
	}
}


internal static class AuthenticationServiceExtensions
{
	internal static bool IsInRole(this JwtSecurityToken jwtSecurityToken, string role) =>
		jwtSecurityToken.Claims.Any(x => x.Type == JwtCustomClaimNames.Role && x.Value == role);
}