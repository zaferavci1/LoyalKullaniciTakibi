namespace LoyalKullaniciTakip.Data
{
    public class EgitimBilgileri
    {
        public int EgitimKayitID { get; set; }
        public int PersonelID { get; set; }
        public string Seviye { get; set; } = string.Empty;
        public string OkulAdi { get; set; } = string.Empty;
        public string Bolum { get; set; } = string.Empty;
        public int MezuniyetYili { get; set; }

        // Navigation property
        public Personel Personel { get; set; } = null!;
    }
}
