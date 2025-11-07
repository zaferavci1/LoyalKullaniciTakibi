namespace LoyalKullaniciTakip.Data.Lookups
{
    /// <summary>
    /// Kıdem yılına göre yıllık izin hak edişlerini tanımlayan lookup tablosu.
    /// Türk İş Kanunu'na göre kıdem yılı arttıkça izin günü artar.
    /// </summary>
    public class Lookup_KidemIzinHakedis
    {
        public int KidemIzinID { get; set; }
        public int MinKidemYili { get; set; }
        public int MaxKidemYili { get; set; }
        public int HakedilenGunSayisi { get; set; }
    }
}

