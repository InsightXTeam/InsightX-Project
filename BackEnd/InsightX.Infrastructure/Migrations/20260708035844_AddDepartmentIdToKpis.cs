using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InsightX.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDepartmentIdToKpis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                table: "KPIs",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_KPIs_DepartmentId",
                table: "KPIs",
                column: "DepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_KPIs_Departments_DepartmentId",
                table: "KPIs",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KPIs_Departments_DepartmentId",
                table: "KPIs");

            migrationBuilder.DropIndex(
                name: "IX_KPIs_DepartmentId",
                table: "KPIs");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "KPIs");
        }
    }
}
