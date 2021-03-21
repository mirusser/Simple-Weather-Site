using Microsoft.EntityFrameworkCore.Migrations;

namespace CitiesService.Migrations
{
    public partial class UpdatedColumnNameInCityInfo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Country",
                table: "CityInfos",
                newName: "CountryCode");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CountryCode",
                table: "CityInfos",
                newName: "Country");
        }
    }
}
