using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LoyalKullaniciTakip.Migrations
{
    /// <inheritdoc />
    public partial class AddPuantajGunlukTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PuantajGunluk",
                columns: table => new
                {
                    PuantajID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PersonelID = table.Column<int>(type: "integer", nullable: false),
                    Tarih = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    PuantajDurumID = table.Column<int>(type: "integer", nullable: false),
                    MesaiSaati = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Aciklama = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PuantajGunluk", x => x.PuantajID);
                    table.ForeignKey(
                        name: "FK_PuantajGunluk_Lookup_PuantajDurumlari_PuantajDurumID",
                        column: x => x.PuantajDurumID,
                        principalTable: "Lookup_PuantajDurumlari",
                        principalColumn: "PuantajDurumID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PuantajGunluk_Personeller_PersonelID",
                        column: x => x.PersonelID,
                        principalTable: "Personeller",
                        principalColumn: "PersonelID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PuantajGunluk_PersonelID_Tarih",
                table: "PuantajGunluk",
                columns: new[] { "PersonelID", "Tarih" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PuantajGunluk_PuantajDurumID",
                table: "PuantajGunluk",
                column: "PuantajDurumID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PuantajGunluk");
        }
    }
}
