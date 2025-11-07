using LoyalKullaniciTakip.Data.Lookups;

namespace LoyalKullaniciTakip.Data
{
    public class MuhasebeHareketleri
    {
        public int HareketID { get; set; }
        public int PersonelID { get; set; }
        public int HareketTipiID { get; set; }
        public decimal Tutar { get; set; }
        public DateTime Tarih { get; set; }
        public string Aciklama { get; set; } = string.Empty;

        // Navigation properties
        public Personel Personel { get; set; } = null!;
        public Lookup_MuhasebeHareketTipi HareketTipi { get; set; } = null!;
    }
}

