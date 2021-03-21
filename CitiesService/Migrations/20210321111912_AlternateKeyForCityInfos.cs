using Microsoft.EntityFrameworkCore.Migrations;

namespace CitiesService.Migrations
{
    public partial class AlternateKeyForCityInfos : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddUniqueConstraint(
                name: "AlternateKey_CityId",
                table: "CityInfos",
                column: "CityId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AlternateKey_CityId",
                table: "CityInfos");
        }
    }
}
