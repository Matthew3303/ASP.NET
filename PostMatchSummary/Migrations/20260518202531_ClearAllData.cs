using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PostMatchSummary.Migrations
{
    public partial class ClearAllData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM Players");
            migrationBuilder.Sql("DELETE FROM Teams");
            migrationBuilder.Sql("DELETE FROM Matches");
            migrationBuilder.Sql("DELETE FROM Champions");

            migrationBuilder.Sql("DBCC CHECKIDENT ('Players', RESEED, 0)");
            migrationBuilder.Sql("DBCC CHECKIDENT ('Teams', RESEED, 0)");
            migrationBuilder.Sql("DBCC CHECKIDENT ('Matches', RESEED, 0)");
            migrationBuilder.Sql("DBCC CHECKIDENT ('Champions', RESEED, 0)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}