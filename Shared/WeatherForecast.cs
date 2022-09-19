namespace BlazorTemplate.Shared.Contracts;

public class WeatherForecast
{
	public DateTime Date		{ get; set; }
	public int TemperatureC		{ get; set; }
	public string? Summary		{ get; set; }
	public int TemperatureF		{ get => 32 + (int)(TemperatureC / 0.5556); }
}