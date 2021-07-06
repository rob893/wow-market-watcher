using Microsoft.EntityFrameworkCore.Migrations;

namespace WoWMarketWatcher.API.Migrations
{
    public partial class ATSIndexRemoval : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuctionTimeSeries_ConnectedRealms_ConnectedRealmId",
                table: "AuctionTimeSeries");

            migrationBuilder.DropForeignKey(
                name: "FK_AuctionTimeSeries_WoWItems_WoWItemId",
                table: "AuctionTimeSeries");

            migrationBuilder.DropIndex(
                name: "IX_AuctionTimeSeries_ConnectedRealmId",
                table: "AuctionTimeSeries");

            migrationBuilder.DropIndex(
                name: "IX_AuctionTimeSeries_WoWItemId",
                table: "AuctionTimeSeries");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AuctionTimeSeries_ConnectedRealmId",
                table: "AuctionTimeSeries",
                column: "ConnectedRealmId");

            migrationBuilder.CreateIndex(
                name: "IX_AuctionTimeSeries_WoWItemId",
                table: "AuctionTimeSeries",
                column: "WoWItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_AuctionTimeSeries_ConnectedRealms_ConnectedRealmId",
                table: "AuctionTimeSeries",
                column: "ConnectedRealmId",
                principalTable: "ConnectedRealms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AuctionTimeSeries_WoWItems_WoWItemId",
                table: "AuctionTimeSeries",
                column: "WoWItemId",
                principalTable: "WoWItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
