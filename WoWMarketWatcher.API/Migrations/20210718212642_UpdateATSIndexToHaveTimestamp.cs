using Microsoft.EntityFrameworkCore.Migrations;

namespace WoWMarketWatcher.API.Migrations
{
    public partial class UpdateATSIndexToHaveTimestamp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AuctionTimeSeries_Timestamp",
                table: "AuctionTimeSeries",
                column: "Timestamp");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AuctionTimeSeries_Timestamp",
                table: "AuctionTimeSeries");
        }
    }
}
