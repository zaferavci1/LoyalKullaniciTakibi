using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoyalKullaniciTakip.Data.Lookups
{
    /// <summary>
    /// Ek mesai katsayıları lookup tablosu
    /// Hafta içi, Cumartesi, Pazar/Tatil gibi günlerin katsayılarını tutar
    /// </summary>
    public class Lookup_EkMesaiKatsayilari
    {
        [Key]
        public int KatsayiID { get; set; }

        /// <summary>
        /// Gün tipi kodu: HaftaIci, Cumartesi, Pazar
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string GunTipi { get; set; } = string.Empty;

        /// <summary>
        /// Ek mesai katsayısı (ör: 1.5, 1.75, 2.0)
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Katsayi { get; set; }

        /// <summary>
        /// Açıklama (ör: "Hafta İçi Fazla Mesai", "Pazar/Tatil Çalışması")
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Aciklama { get; set; } = string.Empty;
    }
}
