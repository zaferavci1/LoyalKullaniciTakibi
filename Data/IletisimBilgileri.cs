using LoyalKullaniciTakip.Data.Lookups;

namespace LoyalKullaniciTakip.Data
{
    public class IletisimBilgileri
    {
        public int IletisimID { get; set; }
        public int PersonelID { get; set; }
        public int IletisimTipiID { get; set; }
        public string Deger { get; set; } = string.Empty;

        // Navigation properties
        public Personel Personel { get; set; } = null!;
        public Lookup_IletisimTipleri IletisimTipi { get; set; } = null!;
    }
}
