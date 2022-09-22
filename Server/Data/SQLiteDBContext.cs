using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace BlazorTemplate.Server.Data;

internal class SQLiteDBContext : IdentityDbContext<IdentityUserEx>
{
	public SQLiteDBContext(DbContextOptions<SQLiteDBContext> options) : base(options) =>
		Database.EnsureCreated();
}