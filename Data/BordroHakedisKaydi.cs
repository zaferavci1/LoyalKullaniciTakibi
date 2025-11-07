namespace LoyalKullaniciTakip.Data
{
    /// <summary>
    /// Aylık bordro hakedişlerinin kalıcı kayıtlarını tutan tablo.
    /// Her personel için aylık hesaplanan maaş bilgileri saklanır.
    /// </summary>
    public class BordroHakedisKaydi
    {
        public int KayitID { get; set; }
        public int PersonelID { get; set; }
        public int Yil { get; set; }
        public int Ay { get; set; }
        public DateTime HesaplananTarih { get; set; }
        public decimal BrutMaas { get; set; }
        public decimal FazlaMesaiUcreti { get; set; }
        public decimal KesintiTutari { get; set; }
        public decimal NetHakedis { get; set; }

        // Navigation properties
        public Personel Personel { get; set; } = null!;
    }
}

