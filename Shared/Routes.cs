namespace BlazorTemplate.Shared;

public struct Routes
{
	private const string Root			= "api";
	private const string Version		= "v1";
	private const string Base			= $"{Root}/{Version}";

	public static bool PointsToIdentity(string? url) =>
		url is not null && url.Contains($"{Base}/identity/", StringComparison.InvariantCultureIgnoreCase);

	public struct Identity
	{
		public const string Register	= $"{Base}/identity/register";
		public const string Login		= $"{Base}/identity/login";
		public const string Logout		= $"{Base}/identity/logout";
		public const string Refresh		= $"{Base}/identity/refresh";
	}
}