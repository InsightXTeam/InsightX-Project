using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InsightX.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReportIdToAlerts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReportId",
                table: "Alerts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_ReportId",
                table: "Alerts",
                column: "ReportId");

            migrationBuilder.AddForeignKey(
                name: "FK_Alerts_Reports_ReportId",
                table: "Alerts",
                column: "ReportId",
                principalTable: "Reports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Alerts_Reports_ReportId",
                table: "Alerts");

            migrationBuilder.DropIndex(
                name: "IX_Alerts_ReportId",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "ReportId",
                table: "Alerts");
        }
    }
}
