using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoyalKullaniciTakip.Migrations
{
    /// <inheritdoc />
    public partial class SeedPuantajDurumlari : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Lookup_PuantajDurumlari",
                columns: new[] { "PuantajDurumID", "Kod", "Tanim" },
                values: new object[,]
                {
                    { 1, "Ç", "Çalıştı" },
                    { 2, "R", "Raporlu" },
                    { 3, "Yİ", "Yıllık İzin" },
                    { 4, "Mİ", "Mazeret İzni" },
                    { 5, "Üİ", "Ücretsiz İzin" },
                    { 6, "İH", "İş Handikapı" },
                    { 7, "T", "Tatil" },
                    { 8, "CT", "Cumartesi Tatil" },
                    { 9, "P", "Pazar" },
                    { 10, "FM", "Fazla Mesai" },
                    { 11, "Gİ", "Genel Tatil" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Lookup_PuantajDurumlari",
                keyColumn: "PuantajDurumID",
                keyValues: new object[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 });
        }
    }
}
