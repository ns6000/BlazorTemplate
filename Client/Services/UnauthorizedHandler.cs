using System.Net;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorTemplate.Shared;


namespace BlazorTemplate.Client.Services;

public class UnauthorizedHandler : DelegatingHandler
{
	private readonly string baseUrl;
	private readonly NavigationManager navigationManager;

	public UnauthorizedHandler(IWebAssemblyHostEnvironment webAssemblyHostEnvironment, NavigationManager navigationManager)
	{
		baseUrl					= webAssemblyHostEnvironment.BaseAddress;
		this.navigationManager	= navigationManager;
	}

	protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

		if(response.StatusCode == HttpStatusCode.Unauthorized && !Routes.PointsToIdentity(request.RequestUri?.AbsolutePath))
			navigationManager.NavigateTo(baseUrl);

		return response;
	}
}