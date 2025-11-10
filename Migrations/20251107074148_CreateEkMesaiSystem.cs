using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LoyalKullaniciTakip.Migrations
{
    /// <inheritdoc />
    public partial class CreateEkMesaiSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EkMesaiKayitlari",
                columns: table => new
                {
                    EkMesaiID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PersonelID = table.Column<int>(type: "integer", nullable: false),
                    Tarih = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    EkMesaiSaati = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    GunTipi = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Katsayi = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    SaatlikUcret = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    HesaplananTutar = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Aciklama = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EkMesaiKayitlari", x => x.EkMesaiID);
                    table.ForeignKey(
                        name: "FK_EkMesaiKayitlari_Personeller_PersonelID",
                        column: x => x.PersonelID,
                        principalTable: "Personeller",
                        principalColumn: "PersonelID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Lookup_EkMesaiKatsayilari",
                columns: table => new
                {
                    KatsayiID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GunTipi = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Katsayi = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Aciklama = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lookup_EkMesaiKatsayilari", x => x.KatsayiID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EkMesaiKayitlari_PersonelID_Tarih",
                table: "EkMesaiKayitlari",
                columns: new[] { "PersonelID", "Tarih" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lookup_EkMesaiKatsayilari_GunTipi",
                table: "Lookup_EkMesaiKatsayilari",
                column: "GunTipi",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EkMesaiKayitlari");

            migrationBuilder.DropTable(
                name: "Lookup_EkMesaiKatsayilari");
        }
    }
}
