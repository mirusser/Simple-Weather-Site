﻿using Microsoft.EntityFrameworkCore.Migrations;

namespace CitiesService.Infrastructure.Migrations;

public partial class Init : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "CityInfos",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                CityId = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                State = table.Column<string>(type: "nvarchar(max)", nullable: true),
                CountryCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Lon = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                Lat = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CityInfos", x => x.Id);
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "CityInfos");
    }
}
