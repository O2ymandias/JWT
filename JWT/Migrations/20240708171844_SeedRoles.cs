using JWT.Data.Helpers;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JWT.Migrations
{
	/// <inheritdoc />
	public partial class SeedRoles : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.InsertData(
				table: "AspNetRoles",
				columns: ["Id", "Name", "NormalizedName", "ConcurrencyStamp"],
				values: [Guid.NewGuid().ToString(), Roles.Admin, Roles.Admin.ToUpper(), Guid.NewGuid().ToString()]
				);

			migrationBuilder.InsertData(
				table: "AspNetRoles",
				columns: ["Id", "Name", "NormalizedName", "ConcurrencyStamp"],
				values: [Guid.NewGuid().ToString(), Roles.User, Roles.User.ToUpper(), Guid.NewGuid().ToString()]
				);

		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.Sql("DELETE FROM AspNetRoles");
		}
	}
}
