using LoyalKullaniciTakip.Data.Lookups;

namespace LoyalKullaniciTakip.Data
{
    public class PuantajGunluk
    {
        public int PuantajID { get; set; }
        public int PersonelID { get; set; }
        public DateTime Tarih { get; set; }
        public int PuantajDurumID { get; set; }
        public decimal? MesaiSaati { get; set; }
        public string Aciklama { get; set; } = string.Empty;

        /// <summary>
        /// Personelin o gün hangi departmanda çalıştığı
        /// Nullable - Mevcut kayıtlar için geriye dönük uyumluluk
        /// </summary>
        public int? DepartmanID { get; set; }

        // Navigation properties
        public Personel Personel { get; set; } = null!;
        public Lookup_PuantajDurumlari PuantajDurum { get; set; } = null!;
        public Lookup_Departmanlar? Departman { get; set; }
    }
}

