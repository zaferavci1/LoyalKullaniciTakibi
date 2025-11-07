using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LoyalKullaniciTakip.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Personeller",
                columns: table => new
                {
                    PersonelID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Ad = table.Column<string>(type: "text", nullable: false),
                    Soyad = table.Column<string>(type: "text", nullable: false),
                    TCKimlikNo = table.Column<string>(type: "text", nullable: false),
                    DogumTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Cinsiyet = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Personeller", x => x.PersonelID);
                });

            migrationBuilder.CreateTable(
                name: "Personel_Detay_Muhasebe",
                columns: table => new
                {
                    PersonelID = table.Column<int>(type: "integer", nullable: false),
                    TemelMaas = table.Column<decimal>(type: "numeric", nullable: false),
                    MaasTipi = table.Column<string>(type: "text", nullable: false),
                    IBAN = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Personel_Detay_Muhasebe", x => x.PersonelID);
                    table.ForeignKey(
                        name: "FK_Personel_Detay_Muhasebe_Personeller_PersonelID",
                        column: x => x.PersonelID,
                        principalTable: "Personeller",
                        principalColumn: "PersonelID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Personel_Detay_SGK",
                columns: table => new
                {
                    PersonelID = table.Column<int>(type: "integer", nullable: false),
                    IseGirisTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CalismaTipiID = table.Column<int>(type: "integer", nullable: false),
                    SGKMeslekKoduID = table.Column<int>(type: "integer", nullable: false),
                    IsyeriSicilNo = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Personel_Detay_SGK", x => x.PersonelID);
                    table.ForeignKey(
                        name: "FK_Personel_Detay_SGK_Personeller_PersonelID",
                        column: x => x.PersonelID,
                        principalTable: "Personeller",
                        principalColumn: "PersonelID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Personel_Detay_Muhasebe_IBAN",
                table: "Personel_Detay_Muhasebe",
                column: "IBAN",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Personeller_TCKimlikNo",
                table: "Personeller",
                column: "TCKimlikNo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Personel_Detay_Muhasebe");

            migrationBuilder.DropTable(
                name: "Personel_Detay_SGK");

            migrationBuilder.DropTable(
                name: "Personeller");
        }
    }
}
