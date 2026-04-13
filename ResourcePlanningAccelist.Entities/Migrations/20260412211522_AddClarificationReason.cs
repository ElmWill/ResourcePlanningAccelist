using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResourcePlanningAccelist.Entities.Migrations
{
    /// <inheritdoc />
    public partial class AddClarificationReason : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClarificationReason",
                table: "GmDecisions",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClarificationReason",
                table: "GmDecisions");
        }
    }
}
