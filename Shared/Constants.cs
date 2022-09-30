namespace BlazorTemplate.Shared;

public struct Constants
{
#if DEBUG
		public const bool DEBUG				= true;
#else
		public const bool DEBUG				= false;
#endif
}

public struct JwtCustomClaimNames
{
	public const string Id					= "id";
	public const string Role				= "role";
}

public struct Roles
{
	public const string Administrator		= "Administrator";
	public const string User				= "User";
}