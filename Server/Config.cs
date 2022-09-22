internal class DatabaseSettings
{
	public string? SQLite					{ get; init; }
}

internal class JWTSettings
{
	public string Domain					{ get; init; } = "/";
	public string? SecretKey				{ get; init; }
	public ushort TokenLifetimeSec			{ get; init; } = 600;
	public ushort RefreshTokenLifetimeHours	{ get; init; } = 10;
	public ushort RefreshTokenSize			{ get; init; } = 64;
}

internal class Config
{
	public DatabaseSettings Database		{ get; init; } = new();
	public JWTSettings JWT					{ get; init; } = new();
	public string AllowedHosts				{ get; init; } = "*";
}