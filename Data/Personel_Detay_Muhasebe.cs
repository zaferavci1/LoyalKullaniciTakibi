namespace LoyalKullaniciTakip.Data
{
    public class Personel_Detay_Muhasebe
    {
        public int PersonelID { get; set; }
        public decimal TemelMaas { get; set; }
        public string MaasTipi { get; set; } = string.Empty;
        public string IBAN { get; set; } = string.Empty;

        // Navigation property for 1-to-1 relationship
        public Personel Personel { get; set; } = null!;
    }
}
