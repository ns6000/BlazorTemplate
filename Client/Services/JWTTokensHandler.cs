using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using BlazorTemplate.Shared;


namespace BlazorTemplate.Client.Services;

public class JWTTokensHandler : DelegatingHandler
{
	private readonly AuthenticationStateProvider authStateProvider;
	private readonly AppState appState;

	public JWTTokensHandler(AuthenticationStateProvider authStateProvider, AppState appState)
	{
		this.authStateProvider	= authStateProvider;
		this.appState			= appState;
	}

	protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		if(appState.AccessToken is not null)
			request.Headers.Authorization = new("Bearer", appState.AccessToken.RawData);

		if(!ApiRoutes.PointsToIdentity(request.RequestUri?.OriginalString))
		{
			AuthenticationState authState	= await authStateProvider.GetAuthenticationStateAsync();
			Claim? expirationClaim			= authState.User.FindFirst(claim => claim.Type.Equals(JwtRegisteredClaimNames.Exp, StringComparison.InvariantCultureIgnoreCase));

			if(expirationClaim is not null)
			{
				DateTime expiryUTC		= DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(expirationClaim.Value)).UtcDateTime;
				TimeSpan timeToExpiry	= expiryUTC - DateTime.UtcNow;

				if(timeToExpiry.TotalSeconds < 60)
				{
					HttpRequestMessage refreshRequest	= new(HttpMethod.Post, ApiRoutes.Identity.Refresh);
					HttpResponseMessage refreshResponse	= await base.SendAsync(refreshRequest, cancellationToken);

					if(refreshResponse.IsSuccessStatusCode)
					{
						string accessToken				= await refreshResponse.Content.ReadAsStringAsync(cancellationToken);
						appState.AccessToken			= new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
						request.Headers.Authorization	= new("Bearer", accessToken);

						((JWTAuthStateProvider)authStateProvider).NotifyUserAuthentication();
					}
				}
			}
		}

		return await base.SendAsync(request, cancellationToken);
	}
}