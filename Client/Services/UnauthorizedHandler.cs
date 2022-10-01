using System.Net;
using Microsoft.AspNetCore.Components;
using BlazorTemplate.Shared;


namespace BlazorTemplate.Client.Services;

public class UnauthorizedHandler : DelegatingHandler
{
	private readonly NavigationManager navigationManager;
	private readonly AppState appState;

	public UnauthorizedHandler(NavigationManager navigationManager, AppState appState)
	{
		this.navigationManager	= navigationManager;
		this.appState			= appState;
	}

	protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

		if(response.StatusCode == HttpStatusCode.Unauthorized && !Routes.PointsToIdentity(request.RequestUri?.OriginalString))
		{
			appState.ClearState();
			navigationManager.NavigateTo("/login");
		}

		return response;
	}
}