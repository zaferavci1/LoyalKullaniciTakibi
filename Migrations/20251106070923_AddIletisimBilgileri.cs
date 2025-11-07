using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LoyalKullaniciTakip.Migrations
{
    /// <inheritdoc />
    public partial class AddIletisimBilgileri : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Lookup_IletisimTipleri",
                columns: table => new
                {
                    IletisimTipiID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Tanim = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lookup_IletisimTipleri", x => x.IletisimTipiID);
                });

            migrationBuilder.CreateTable(
                name: "IletisimBilgileri",
                columns: table => new
                {
                    IletisimID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PersonelID = table.Column<int>(type: "integer", nullable: false),
                    IletisimTipiID = table.Column<int>(type: "integer", nullable: false),
                    Deger = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IletisimBilgileri", x => x.IletisimID);
                    table.ForeignKey(
                        name: "FK_IletisimBilgileri_Lookup_IletisimTipleri_IletisimTipiID",
                        column: x => x.IletisimTipiID,
                        principalTable: "Lookup_IletisimTipleri",
                        principalColumn: "IletisimTipiID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IletisimBilgileri_Personeller_PersonelID",
                        column: x => x.PersonelID,
                        principalTable: "Personeller",
                        principalColumn: "PersonelID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IletisimBilgileri_IletisimTipiID",
                table: "IletisimBilgileri",
                column: "IletisimTipiID");

            migrationBuilder.CreateIndex(
                name: "IX_IletisimBilgileri_PersonelID",
                table: "IletisimBilgileri",
                column: "PersonelID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IletisimBilgileri");

            migrationBuilder.DropTable(
                name: "Lookup_IletisimTipleri");
        }
    }
}
