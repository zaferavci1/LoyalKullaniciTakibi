using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LoyalKullaniciTakip.Migrations
{
    /// <inheritdoc />
    public partial class AddEgitimBilgileriTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EgitimBilgileri",
                columns: table => new
                {
                    EgitimKayitID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PersonelID = table.Column<int>(type: "integer", nullable: false),
                    Seviye = table.Column<string>(type: "text", nullable: false),
                    OkulAdi = table.Column<string>(type: "text", nullable: false),
                    Bolum = table.Column<string>(type: "text", nullable: false),
                    MezuniyetYili = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EgitimBilgileri", x => x.EgitimKayitID);
                    table.ForeignKey(
                        name: "FK_EgitimBilgileri_Personeller_PersonelID",
                        column: x => x.PersonelID,
                        principalTable: "Personeller",
                        principalColumn: "PersonelID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EgitimBilgileri_PersonelID",
                table: "EgitimBilgileri",
                column: "PersonelID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EgitimBilgileri");
        }
    }
}
