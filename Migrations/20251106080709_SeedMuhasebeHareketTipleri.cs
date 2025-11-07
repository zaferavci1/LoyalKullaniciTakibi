using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoyalKullaniciTakip.Migrations
{
    /// <inheritdoc />
    public partial class SeedMuhasebeHareketTipleri : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Lookup_MuhasebeHareketTipi",
                columns: new[] { "HareketTipiID", "Tanim" },
                values: new object[,]
                {
                    { 1, "Avans" },
                    { 2, "Masraf" },
                    { 3, "Prim" },
                    { 4, "Kesinti" },
                    { 5, "İkramiye" },
                    { 6, "Fazla Mesai" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Lookup_MuhasebeHareketTipi",
                keyColumn: "HareketTipiID",
                keyValues: new object[] { 1, 2, 3, 4, 5, 6 });
        }
    }
}
