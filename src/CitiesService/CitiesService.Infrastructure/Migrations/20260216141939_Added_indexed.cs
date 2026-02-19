using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CitiesService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Added_indexed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "CityInfos",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_CityInfos_CityId",
                table: "CityInfos",
                column: "CityId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CityInfos_Name",
                table: "CityInfos",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CityInfos_CityId",
                table: "CityInfos");

            migrationBuilder.DropIndex(
                name: "IX_CityInfos_Name",
                table: "CityInfos");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "CityInfos",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
