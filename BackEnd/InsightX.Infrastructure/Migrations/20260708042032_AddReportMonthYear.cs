using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InsightX.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReportMonthYear : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReportMonth",
                table: "Reports",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ReportYear",
                table: "Reports",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReportMonth",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "ReportYear",
                table: "Reports");
        }
    }
}
