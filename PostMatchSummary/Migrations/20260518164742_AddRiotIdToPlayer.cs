using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PostMatchSummary.Migrations
{
    /// <inheritdoc />
    public partial class AddRiotIdToPlayer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RiotId",
                table: "Players",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RiotId",
                table: "Players");
        }
    }
}
