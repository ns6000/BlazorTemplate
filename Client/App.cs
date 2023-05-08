using System.Globalization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using BlazorTemplate.Client;
using BlazorTemplate.Client.Services;
using BlazorTemplate.Shared;


WebAssemblyHostBuilder? builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents
	.Add<Root>("#app");
builder.RootComponents
	.Add<HeadOutlet>("head::after");
builder.Services
	.AddSingleton<AppState>();
builder.Services
	.AddAuthorizationCore();
builder.Services
	.AddScoped<AuthenticationStateProvider, JWTAuthStateProvider>();
builder.Services
	.AddScoped<AuthenticationService>();
builder.Services
	.AddScoped<JWTTokensHandler>();
builder.Services
	.AddScoped<UnauthorizedHandler>();
builder.Services
	.AddHttpClient("ApiClient", client => {
		client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
		client.Timeout = TimeSpan.FromSeconds(7);
	})
	.AddHttpMessageHandler<JWTTokensHandler>()
	.AddHttpMessageHandler<UnauthorizedHandler>();
builder.Services
	.AddScoped(service => service
		.GetRequiredService<IHttpClientFactory>()
		.CreateClient("ApiClient")
	);


WebAssemblyHost host = builder.Build();

IJSRuntime js		= host.Services.GetRequiredService<IJSRuntime>();
string language		= (await js.InvokeAsync<string>("blazorCulture.get"))
	?? (await js.InvokeAsync<string>("browserLanguage"))
	?? "cs";
CultureInfo culture	= new(language.Length > 2 ? language[..2] : language);

CultureInfo.DefaultThreadCurrentCulture		= culture;
CultureInfo.DefaultThreadCurrentUICulture	= culture;
await js.InvokeVoidAsync("blazorCulture.set", language);

await host.RunAsync();