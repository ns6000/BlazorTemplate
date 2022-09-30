using System.Globalization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Net.Http.Headers;


namespace BlazorTemplate.Server.Pages;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
[IgnoreAntiforgeryToken]
public class BlazorBootstrapperModel : PageModel
{
	public string Language { get; private set; } = "en";

	public void OnGet()
	{
		string? acceptLanguage = Request?.Headers[HeaderNames.AcceptLanguage];
		if(!string.IsNullOrEmpty(acceptLanguage) && acceptLanguage.Length >= 2)
			Language = acceptLanguage[..2].ToLower();

		CultureInfo culture							= new(Language);
		CultureInfo.DefaultThreadCurrentCulture		= culture;
		CultureInfo.DefaultThreadCurrentUICulture	= culture;
	}
}