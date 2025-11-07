using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoyalKullaniciTakip.Migrations
{
    /// <inheritdoc />
    public partial class SeedIzinTipleri : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Lookup_IzinTipleri",
                columns: new[] { "IzinTipiID", "Tanim", "YillikIzindenDuserMi" },
                values: new object[,]
                {
                    { 1, "Yıllık İzin", true },
                    { 2, "Mazeret İzni (Evlilik)", false },
                    { 3, "Mazeret İzni (Ölüm)", false },
                    { 4, "Mazeret İzni (Doğum)", false },
                    { 5, "Ücretsiz İzin", false },
                    { 6, "Hastalık İzni (Raporlu)", false },
                    { 7, "Babalık İzni", false },
                    { 8, "Doğum İzni", false }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Lookup_IzinTipleri",
                keyColumn: "IzinTipiID",
                keyValues: new object[] { 1, 2, 3, 4, 5, 6, 7, 8 });
        }
    }
}
