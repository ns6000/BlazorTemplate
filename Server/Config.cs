internal class DatabaseSettings
{
	public string? SQLite					{ get; init; }
}

internal class JWTSettings
{
	public string? SecretKey				{ get; init; }
	public ushort ExpiresAfterMin			{ get; init; } = 60;
}

internal class Config
{
	public DatabaseSettings Database		{ get; init; } = new();
	public JWTSettings JWT					{ get; init; } = new();
	public string AllowedHosts				{ get; init; } = "";
}