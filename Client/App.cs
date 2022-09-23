using System.Globalization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using BlazorTemplate.Client;


WebAssemblyHostBuilder? builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<Root>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(_ => new HttpClient {
	BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});


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