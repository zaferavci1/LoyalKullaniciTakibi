using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LoyalKullaniciTakip.Migrations
{
    /// <inheritdoc />
    public partial class AddBelgeler : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Lookup_BelgeKategorileri",
                columns: table => new
                {
                    BelgeKategoriID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Tanim = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lookup_BelgeKategorileri", x => x.BelgeKategoriID);
                });

            migrationBuilder.CreateTable(
                name: "Belgeler",
                columns: table => new
                {
                    BelgeID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PersonelID = table.Column<int>(type: "integer", nullable: false),
                    BelgeKategoriID = table.Column<int>(type: "integer", nullable: false),
                    BelgeAdi = table.Column<string>(type: "text", nullable: false),
                    DosyaYolu = table.Column<string>(type: "text", nullable: false),
                    YuklenmeTarihi = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Belgeler", x => x.BelgeID);
                    table.ForeignKey(
                        name: "FK_Belgeler_Lookup_BelgeKategorileri_BelgeKategoriID",
                        column: x => x.BelgeKategoriID,
                        principalTable: "Lookup_BelgeKategorileri",
                        principalColumn: "BelgeKategoriID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Belgeler_Personeller_PersonelID",
                        column: x => x.PersonelID,
                        principalTable: "Personeller",
                        principalColumn: "PersonelID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Belgeler_BelgeKategoriID",
                table: "Belgeler",
                column: "BelgeKategoriID");

            migrationBuilder.CreateIndex(
                name: "IX_Belgeler_PersonelID",
                table: "Belgeler",
                column: "PersonelID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Belgeler");

            migrationBuilder.DropTable(
                name: "Lookup_BelgeKategorileri");
        }
    }
}
