using LoyalKullaniciTakip.Data.Lookups;

namespace LoyalKullaniciTakip.Data
{
    public class Personel_Detay_SGK
    {
        public int PersonelID { get; set; }
        public DateTime IseGirisTarihi { get; set; }
        public int CalismaTipiID { get; set; }
        public int SGKMeslekKoduID { get; set; }
        public string IsyeriSicilNo { get; set; } = string.Empty;

        // Navigation properties
        public Personel Personel { get; set; } = null!;
        public Lookup_CalismaTipi CalismaTipi { get; set; } = null!;
        public Lookup_MeslekKodlari MeslekKodu { get; set; } = null!;
    }
}
