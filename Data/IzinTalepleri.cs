using LoyalKullaniciTakip.Data.Lookups;

namespace LoyalKullaniciTakip.Data
{
    public class IzinTalepleri
    {
        public int TalepID { get; set; }
        public int PersonelID { get; set; }
        public int IzinTipiID { get; set; }
        public DateTime BaslangicTarihi { get; set; }
        public DateTime BitisTarihi { get; set; }
        public DateTime TalepTarihi { get; set; }
        public string Aciklama { get; set; } = string.Empty;
        public int OnayDurumu { get; set; } // 0:Bekliyor, 1:Onaylandı, 2:Reddedildi
        public int? OnaylayanPersonelID { get; set; }

        // Navigation properties
        public Personel Personel { get; set; } = null!;
        public Lookup_IzinTipleri IzinTipi { get; set; } = null!;
        public Personel? OnaylayanPersonel { get; set; }
    }
}

