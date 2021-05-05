using Microsoft.EntityFrameworkCore.Migrations;

namespace WoWMarketWatcher.API.Migrations
{
    public partial class AddPopulationToConnectedRealm : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Population",
                table: "ConnectedRealms",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Population",
                table: "ConnectedRealms");
        }
    }
}