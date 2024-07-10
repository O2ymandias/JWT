using JWT.Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace JWT.Data
{
	public class ApplicationIdentityDbContext : IdentityDbContext<ApplicationUser>
	{
		public ApplicationIdentityDbContext(DbContextOptions<ApplicationIdentityDbContext> options)
			: base(options)
		{
		}

		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);
			builder
				.Entity<ApplicationUser>()
				.HasMany(appUser => appUser.RefreshTokens)
				.WithOne()
				.IsRequired();
			builder
				.Entity<RefreshToken>()
				.ToTable("RefreshTokens");
		}
	}
}
