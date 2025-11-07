using System.ComponentModel.DataAnnotations;

namespace LoyalKullaniciTakip.Data.Lookups
{
    public class Lookup_GenelAyarlar
    {
        [Key]
        public string AyarKey { get; set; } = string.Empty;
        public string AyarValue { get; set; } = string.Empty;
        public string? Aciklama { get; set; }
    }
}
