using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoyalKullaniciTakip.Migrations
{
    /// <inheritdoc />
    public partial class AddDevamsizPuantajDurumu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // "Devamsız" puantaj durumunu ekle (eğer yoksa)
            migrationBuilder.Sql(@"
                INSERT INTO ""Lookup_PuantajDurumlari"" (""Kod"", ""Tanim"")
                SELECT 'D', 'Devamsız'
                WHERE NOT EXISTS (
                    SELECT 1 FROM ""Lookup_PuantajDurumlari""
                    WHERE ""Kod"" = 'D' OR ""Tanim"" = 'Devamsız'
                );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // "Devamsız" puantaj durumunu sil
            migrationBuilder.Sql(@"
                DELETE FROM ""Lookup_PuantajDurumlari""
                WHERE ""Kod"" = 'D' AND ""Tanim"" = 'Devamsız';
            ");
        }
    }
}
