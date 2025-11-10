using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoyalKullaniciTakip.Data
{
    /// <summary>
    /// Ek Mesai (Overtime) kayıtları tablosu
    /// Personellerin fazla mesai çalışmalarını tutar
    /// </summary>
    public class EkMesaiKayitlari
    {
        [Key]
        public int EkMesaiID { get; set; }

        [Required]
        public int PersonelID { get; set; }

        [Required]
        public DateTime Tarih { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal EkMesaiSaati { get; set; }

        /// <summary>
        /// Gün tipi: HaftaIci, Cumartesi, Pazar
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string GunTipi { get; set; } = string.Empty;

        /// <summary>
        /// Ek mesai katsayısı (1.5, 1.75, 2.0 gibi)
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Katsayi { get; set; }

        /// <summary>
        /// Personelin saatlik ücreti (Maaş / 225)
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal SaatlikUcret { get; set; }

        /// <summary>
        /// Hesaplanan toplam tutar: SaatlikUcret × EkMesaiSaati × Katsayi
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal HesaplananTutar { get; set; }

        [MaxLength(500)]
        public string Aciklama { get; set; } = string.Empty;

        /// <summary>
        /// Kaydın oluşturulma tarihi (audit trail)
        /// </summary>
        [Required]
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual Personel Personel { get; set; } = null!;
    }
}
