using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InsightX.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyIdToReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "Reports",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "Reports");
        }
    }
}
