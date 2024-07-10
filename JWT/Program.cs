using JWT.Data;
using JWT.Data.Models;
using JWT.Helpers;
using JWT.Services.Contract;
using JWT.Services.Implementation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace JWT
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);


			#region Add services to the IOC container.

			builder.Services.AddControllers();
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen();

			builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JWT"));

			builder.Services.AddDbContext<ApplicationIdentityDbContext>(options =>
			{
				options
				.UseLazyLoadingProxies()
				.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
			});


			builder.Services
				.AddIdentity<ApplicationUser, IdentityRole>()
				.AddEntityFrameworkStores<ApplicationIdentityDbContext>();

			builder.Services.AddAuthentication(options =>
			{
				options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
			})
				.AddJwtBearer(options =>
				{
					options.SaveToken = false;

					if (builder.Environment.IsDevelopment())
						options.RequireHttpsMetadata = false;

					options.TokenValidationParameters = new TokenValidationParameters()
					{
						ValidateIssuer = true,
						ValidIssuer = builder.Configuration["JWT:Issuer"],

						ValidateAudience = true,
						ValidAudience = builder.Configuration["JWT:Audience"],

						ValidateIssuerSigningKey = true,
						IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:SecurityKey"]!)),

						ValidateLifetime = true,
						ClockSkew = TimeSpan.Zero
					};
				});

			builder.Services.AddScoped(typeof(IAuthService), typeof(AuthService));
			builder.Services.AddScoped(typeof(ITokenService), typeof(TokenService));

			#endregion

			var app = builder.Build();

			#region Configure the HTTP request pipeline

			if (app.Environment.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI();
			}

			app.UseHttpsRedirection();

			app.UseAuthorization();


			app.MapControllers();

			app.Run();

			#endregion
		}
	}
}
