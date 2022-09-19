namespace BlazorTemplate.Shared.Routes;

public static class Routes
{
	private const string Root			= "api";
	private const string Version		= "v1";
	private const string Base			= $"{Root}/{Version}";

	public static class Identity
	{
		public const string Register	= $"{Base}/identity/register";
		public const string Login		= $"{Base}/identity/login";
		public const string Logout		= $"{Base}/identity/logout";
	}
}