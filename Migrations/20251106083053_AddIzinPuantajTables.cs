using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LoyalKullaniciTakip.Migrations
{
    /// <inheritdoc />
    public partial class AddIzinPuantajTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Lookup_IzinTipleri",
                columns: table => new
                {
                    IzinTipiID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Tanim = table.Column<string>(type: "text", nullable: false),
                    YillikIzindenDuserMi = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lookup_IzinTipleri", x => x.IzinTipiID);
                });

            migrationBuilder.CreateTable(
                name: "Lookup_PuantajDurumlari",
                columns: table => new
                {
                    PuantajDurumID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Kod = table.Column<string>(type: "text", nullable: false),
                    Tanim = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lookup_PuantajDurumlari", x => x.PuantajDurumID);
                });

            migrationBuilder.CreateTable(
                name: "IzinTalepleri",
                columns: table => new
                {
                    TalepID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PersonelID = table.Column<int>(type: "integer", nullable: false),
                    IzinTipiID = table.Column<int>(type: "integer", nullable: false),
                    BaslangicTarihi = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    BitisTarihi = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    TalepTarihi = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Aciklama = table.Column<string>(type: "text", nullable: false),
                    OnayDurumu = table.Column<int>(type: "integer", nullable: false),
                    OnaylayanPersonelID = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IzinTalepleri", x => x.TalepID);
                    table.ForeignKey(
                        name: "FK_IzinTalepleri_Lookup_IzinTipleri_IzinTipiID",
                        column: x => x.IzinTipiID,
                        principalTable: "Lookup_IzinTipleri",
                        principalColumn: "IzinTipiID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IzinTalepleri_Personeller_OnaylayanPersonelID",
                        column: x => x.OnaylayanPersonelID,
                        principalTable: "Personeller",
                        principalColumn: "PersonelID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IzinTalepleri_Personeller_PersonelID",
                        column: x => x.PersonelID,
                        principalTable: "Personeller",
                        principalColumn: "PersonelID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IzinTalepleri_IzinTipiID",
                table: "IzinTalepleri",
                column: "IzinTipiID");

            migrationBuilder.CreateIndex(
                name: "IX_IzinTalepleri_OnaylayanPersonelID",
                table: "IzinTalepleri",
                column: "OnaylayanPersonelID");

            migrationBuilder.CreateIndex(
                name: "IX_IzinTalepleri_PersonelID",
                table: "IzinTalepleri",
                column: "PersonelID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IzinTalepleri");

            migrationBuilder.DropTable(
                name: "Lookup_PuantajDurumlari");

            migrationBuilder.DropTable(
                name: "Lookup_IzinTipleri");
        }
    }
}
