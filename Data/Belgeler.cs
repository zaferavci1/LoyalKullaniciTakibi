using LoyalKullaniciTakip.Data.Lookups;

namespace LoyalKullaniciTakip.Data
{
    public class Belgeler
    {
        public int BelgeID { get; set; }
        public int PersonelID { get; set; }
        public int BelgeKategoriID { get; set; }
        public string BelgeAdi { get; set; } = string.Empty;
        public string DosyaYolu { get; set; } = string.Empty;
        public DateTime YuklenmeTarihi { get; set; }

        // Navigation properties
        public Personel Personel { get; set; } = null!;
        public Lookup_BelgeKategorileri BelgeKategori { get; set; } = null!;
    }
}
