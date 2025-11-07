namespace LoyalKullaniciTakip.Data
{
    public class Zimmet
    {
        public int ZimmetID { get; set; }
        public int PersonelID { get; set; }
        public string DemirbasAdi { get; set; } = string.Empty;
        public string MarkaModel { get; set; } = string.Empty;
        public string SeriNo { get; set; } = string.Empty;
        public DateTime VerilisTarihi { get; set; }
        public DateTime? IadeTarihi { get; set; }

        // Navigation property
        public Personel Personel { get; set; } = null!;
    }
}

