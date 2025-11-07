namespace LoyalKullaniciTakip.Data
{
    public class Personel_Detay_SGK
    {
        public int PersonelID { get; set; }
        public DateTime IseGirisTarihi { get; set; }
        public int CalismaTipiID { get; set; }
        public int SGKMeslekKoduID { get; set; }
        public string IsyeriSicilNo { get; set; } = string.Empty;

        // Navigation property for 1-to-1 relationship
        public Personel Personel { get; set; } = null!;
    }
}
