using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResourcePlanningAccelist.Entities.Migrations
{
    /// <inheritdoc />
    public partial class EnableContractHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmployeeContracts_EmployeeId",
                table: "EmployeeContracts");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeContracts_EmployeeId",
                table: "EmployeeContracts",
                column: "EmployeeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmployeeContracts_EmployeeId",
                table: "EmployeeContracts");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeContracts_EmployeeId",
                table: "EmployeeContracts",
                column: "EmployeeId",
                unique: true);
        }
    }
}
