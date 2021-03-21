using Microsoft.EntityFrameworkCore.Migrations;

namespace CitiesService.Migrations
{
    public partial class AddedIndexForCityInfos_CityId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_CityInfos_CityId",
                table: "CityInfos",
                column: "CityId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CityInfos_CityId",
                table: "CityInfos");
        }
    }
}
