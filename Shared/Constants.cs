namespace BlazorTemplate.Shared;

public static class Constants
{
#if DEBUG
		public const bool DEBUG		= true;
#else
		public const bool DEBUG		= false;
#endif
}
