using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using FluentValidation;
using BlazorTemplate.Shared.Contracts;
using BlazorTemplate.Server.Services;
using BlazorTemplate.Server.Data;

namespace BlazorTemplate.Server;

internal class App
{
	public static Config Config { get; private set; } = default!;

	public static async Task Main(string[] args)
	{
		WebApplicationBuilder? builder = WebApplication.CreateBuilder(args);

		Config = new ConfigurationBuilder()
			.AddJsonFile("appsettings.json")
			.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
			.Build()
			.Get<Config>();

		builder.Services
			.AddScoped<IdentityService>();
		builder.Services
			.AddControllers();
		//builder.Services.AddControllersWithViews();
		//builder.Services.AddRazorPages();
		builder.Services
			.AddDbContext<SQLiteDBContext>(options => {
				options.UseSqlite(Config.Database.SQLite);
			});
		builder.Services
			.AddIdentity<IdentityUser, IdentityRole>()
			.AddEntityFrameworkStores<SQLiteDBContext>();
		builder.Services
			.AddAuthentication(options => {
				options.DefaultScheme				= JwtBearerDefaults.AuthenticationScheme;
				options.DefaultAuthenticateScheme	= JwtBearerDefaults.AuthenticationScheme;
				options.DefaultChallengeScheme		= JwtBearerDefaults.AuthenticationScheme;
			})
			.AddJwtBearer(options => {
				options.SaveToken					= true;
				options.TokenValidationParameters	= new TokenValidationParameters {
					RequireExpirationTime		= true,
					ValidateLifetime			= true,
					ValidateAudience			= true,
					ValidateIssuer				= true,
					ValidateIssuerSigningKey	= true,
					IssuerSigningKey			= new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Config.JWT.SecretKey))
				};
			});
		builder.Services
			.AddValidatorsFromAssemblyContaining<RegistrationRequest>();



		WebApplication? app = builder.Build();

		if(app.Environment.IsDevelopment())
			app.UseWebAssemblyDebugging();
		else
		{
			app.UseExceptionHandler("/Error");
			app.UseHsts(); // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
			app.UseHttpsRedirection();
		}

		app.UseBlazorFrameworkFiles();
		app.UseStaticFiles();
		app.UseAuthentication();
		app.UseRouting();
		app.UseAuthorization();
		//app.MapRazorPages();
		app.MapControllers();
		app.MapFallbackToFile("index.html");

		await app
			.RunAsync();
	}
}