using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using BlazorTemplate.Shared.Contracts;
using BlazorTemplate.Server.Services;
using BlazorTemplate.Server.Data;
using FluentValidation.AspNetCore;


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
			.Get<Config>()!;

		builder.Services
			.AddControllers();
		builder.Services
			.AddFluentValidationAutoValidation()
			.AddFluentValidationClientsideAdapters();
			//.AddFluentValidation(x => x.AutomaticValidationEnabled = true); //.AddFluentValidationAutoValidation();
		//builder.Services.AddControllersWithViews();
		builder.Services.AddRazorPages();
		builder.Services
			.AddDbContext<SQLiteDBContext>(options => {
				options.UseSqlite(Config!.Database.SQLite);
			});
		builder.Services
			.AddIdentity<IdentityUserEx, IdentityRole>()
			.AddEntityFrameworkStores<SQLiteDBContext>();
		builder.Services
			.Configure<IdentityOptions>(options => {
				options.Password.RequiredLength = 8;
			});
		builder.Services
			.AddAuthentication(options => {
				options.DefaultScheme				= JwtBearerDefaults.AuthenticationScheme;
				options.DefaultAuthenticateScheme	= JwtBearerDefaults.AuthenticationScheme;
				options.DefaultChallengeScheme		= JwtBearerDefaults.AuthenticationScheme;
			})
			.AddJwtBearer(options => {
				options.SaveToken					= true;
				options.TokenValidationParameters	= IdentityService.TokenValidationParameters;
			});
		builder.Services
			.AddValidatorsFromAssemblyContaining<RegistrationRequest>();

		builder.Services
			.AddScoped<IdentityService>();



		WebApplication? app = builder.Build();

		if(Constants.DEBUG)
		{
			using IServiceScope scope = app.Services.CreateScope();
			SQLiteDBContext dbContext = scope.ServiceProvider.GetRequiredService<SQLiteDBContext>();

			if(!await dbContext.Roles.AnyAsync())
			{
				await dbContext.Roles.AddAsync(new IdentityRole {
					Name			= Roles.Administrator,
					NormalizedName	= Roles.Administrator.ToUpper(),
				});

				await dbContext.Roles.AddAsync(new IdentityRole {
					Name			= Roles.User,
					NormalizedName	= Roles.User.ToUpper(),
				});

				await dbContext.SaveChangesAsync();
			}
		}

		app.Use(async (context, next) => {
			try
			{
				await next(context);
			}
			catch(Exception ex)
			{
				int statusCode = ex switch {
					ArgumentException			=> StatusCodes.Status400BadRequest,
					UnauthorizedAccessException	=> StatusCodes.Status401Unauthorized,
					MemberAccessException		=> StatusCodes.Status403Forbidden,
					_							=> StatusCodes.Status500InternalServerError
				};

				context.Response.ContentType	= "application/json";
				context.Response.StatusCode		= statusCode;
				await context.Response.WriteAsync(ex.Message);
			}
		});

		if(app.Environment.IsDevelopment())
			app.UseWebAssemblyDebugging();
		else
		{
			//app.UseExceptionHandler("/Error");
			app.UseHsts(); // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
			app.UseHttpsRedirection();
		}

		app.UseBlazorFrameworkFiles();
		app.UseStaticFiles();
		app.UseAuthentication();
		app.UseRouting();
		app.UseAuthorization();
		app.MapRazorPages();
		app.MapControllers();
		//app.MapFallbackToFile("index.html");
		app.MapFallbackToPage("/BlazorBootstrapper");

		await app
			.RunAsync();
	}
}