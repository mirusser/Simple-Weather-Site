using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CitiesService.Migrations.PostgreSql.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgreSql : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CityInfos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CityId = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    State = table.Column<string>(type: "text", nullable: true),
                    CountryCode = table.Column<string>(type: "text", nullable: false),
                    Lon = table.Column<decimal>(type: "numeric(9,6)", nullable: false),
                    Lat = table.Column<decimal>(type: "numeric(9,6)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CityInfos", x => x.Id);
                });

            migrationBuilder.Sql(
                """
                CREATE OR REPLACE FUNCTION set_cityinfos_rowversion()
                RETURNS trigger AS $$
                BEGIN
                    NEW."RowVersion" = decode(md5(random()::text || clock_timestamp()::text || txid_current()::text), 'hex');
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
                """);

            migrationBuilder.Sql(
                """
                CREATE TRIGGER "TR_CityInfos_RowVersion"
                BEFORE INSERT OR UPDATE ON "CityInfos"
                FOR EACH ROW
                EXECUTE FUNCTION set_cityinfos_rowversion();
                """);

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
            migrationBuilder.DropTable(
                name: "CityInfos");

            migrationBuilder.Sql(
                """
                DROP FUNCTION IF EXISTS set_cityinfos_rowversion();
                """);
        }
    }
}
