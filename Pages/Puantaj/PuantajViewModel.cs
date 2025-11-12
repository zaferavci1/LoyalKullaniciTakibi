using LoyalKullaniciTakip.Data;

namespace LoyalKullaniciTakip.Pages.Puantaj
{
    public class PuantajViewModel
    {
        public int Ay { get; set; }
        public int Yil { get; set; }
        public List<PersonelPuantajSatiri> PersonelSatirlari { get; set; } = new List<PersonelPuantajSatiri>();
    }

    public class PersonelPuantajSatiri
    {
        public int PersonelID { get; set; }
        public string PersonelAdSoyad { get; set; } = string.Empty;
        public int? DepartmanID { get; set; } // Personelin varsayılan departmanı
        // Key: Gün (1-31), Value: O günkü puantaj kaydı
        public Dictionary<int, PuantajGunluk?> GunlukKayitlar { get; set; } = new Dictionary<int, PuantajGunluk?>();
    }
}

