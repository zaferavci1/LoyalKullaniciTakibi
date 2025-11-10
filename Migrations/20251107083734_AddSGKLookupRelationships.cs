using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoyalKullaniciTakip.Migrations
{
    /// <inheritdoc />
    public partial class AddSGKLookupRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Personel_Detay_SGK_CalismaTipiID",
                table: "Personel_Detay_SGK",
                column: "CalismaTipiID");

            migrationBuilder.CreateIndex(
                name: "IX_Personel_Detay_SGK_SGKMeslekKoduID",
                table: "Personel_Detay_SGK",
                column: "SGKMeslekKoduID");

            migrationBuilder.AddForeignKey(
                name: "FK_Personel_Detay_SGK_Lookup_CalismaTipi_CalismaTipiID",
                table: "Personel_Detay_SGK",
                column: "CalismaTipiID",
                principalTable: "Lookup_CalismaTipi",
                principalColumn: "CalismaTipiID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Personel_Detay_SGK_Lookup_MeslekKodlari_SGKMeslekKoduID",
                table: "Personel_Detay_SGK",
                column: "SGKMeslekKoduID",
                principalTable: "Lookup_MeslekKodlari",
                principalColumn: "MeslekKoduID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Personel_Detay_SGK_Lookup_CalismaTipi_CalismaTipiID",
                table: "Personel_Detay_SGK");

            migrationBuilder.DropForeignKey(
                name: "FK_Personel_Detay_SGK_Lookup_MeslekKodlari_SGKMeslekKoduID",
                table: "Personel_Detay_SGK");

            migrationBuilder.DropIndex(
                name: "IX_Personel_Detay_SGK_CalismaTipiID",
                table: "Personel_Detay_SGK");

            migrationBuilder.DropIndex(
                name: "IX_Personel_Detay_SGK_SGKMeslekKoduID",
                table: "Personel_Detay_SGK");
        }
    }
}
