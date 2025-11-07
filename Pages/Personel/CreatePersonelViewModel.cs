using System.ComponentModel.DataAnnotations;

namespace LoyalKullaniciTakip.Pages.Personel
{
    public class CreatePersonelViewModel
    {
        // Personel Bilgileri
        [Required(ErrorMessage = "Ad alanı zorunludur")]
        [StringLength(100)]
        [Display(Name = "Ad")]
        public string Ad { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad alanı zorunludur")]
        [StringLength(100)]
        [Display(Name = "Soyad")]
        public string Soyad { get; set; } = string.Empty;

        [Required(ErrorMessage = "TC Kimlik No zorunludur")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "TC Kimlik No 11 karakter olmalıdır")]
        [Display(Name = "TC Kimlik No")]
        public string TCKimlikNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Doğum Tarihi zorunludur")]
        [DataType(DataType.Date)]
        [Display(Name = "Doğum Tarihi")]
        public DateTime DogumTarihi { get; set; }

        [Required(ErrorMessage = "Cinsiyet seçimi zorunludur")]
        [StringLength(10)]
        [Display(Name = "Cinsiyet")]
        public string Cinsiyet { get; set; } = string.Empty;

        [Display(Name = "Departman")]
        public int? DepartmanID { get; set; }

        [Display(Name = "Meslek")]
        public int? MeslekID { get; set; }

        // SGK Detay Bilgileri
        [Required(ErrorMessage = "İşe Giriş Tarihi zorunludur")]
        [DataType(DataType.Date)]
        [Display(Name = "İşe Giriş Tarihi")]
        public DateTime IseGirisTarihi { get; set; }

        [Required(ErrorMessage = "Çalışma Tipi ID zorunludur")]
        [Display(Name = "Çalışma Tipi ID")]
        public int CalismaTipiID { get; set; }

        [Required(ErrorMessage = "SGK Meslek Kodu ID zorunludur")]
        [Display(Name = "SGK Meslek Kodu ID")]
        public int SGKMeslekKoduID { get; set; }

        [Required(ErrorMessage = "İşyeri Sicil No zorunludur")]
        [StringLength(50)]
        [Display(Name = "İşyeri Sicil No")]
        public string IsyeriSicilNo { get; set; } = string.Empty;

        // Muhasebe Detay Bilgileri
        [Required(ErrorMessage = "Temel Maaş zorunludur")]
        [Range(0, double.MaxValue, ErrorMessage = "Temel Maaş 0'dan büyük olmalıdır")]
        [Display(Name = "Temel Maaş")]
        public decimal TemelMaas { get; set; }

        [Required(ErrorMessage = "Maaş Tipi zorunludur")]
        [StringLength(50)]
        [Display(Name = "Maaş Tipi")]
        public string MaasTipi { get; set; } = string.Empty;

        [Required(ErrorMessage = "IBAN zorunludur")]
        [StringLength(34)]
        [Display(Name = "IBAN")]
        public string IBAN { get; set; } = string.Empty;
    }
}
