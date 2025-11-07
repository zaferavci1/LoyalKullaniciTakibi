namespace LoyalKullaniciTakip.Data
{
    public class Personel
    {
        public int PersonelID { get; set; }
        public string Ad { get; set; } = string.Empty;
        public string Soyad { get; set; } = string.Empty;
        public string TCKimlikNo { get; set; } = string.Empty;
        public DateTime DogumTarihi { get; set; }
        public string Cinsiyet { get; set; } = string.Empty;

        // Lookup foreign keys
        public int? DepartmanID { get; set; }
        public int? MeslekID { get; set; }

        // Profile fields
        public string? FotografDosyaYolu { get; set; }

        // Navigation properties for lookups
        public virtual Lookups.Lookup_Departmanlar? Departman { get; set; }
        public virtual Lookups.Lookup_Meslekler? Meslek { get; set; }

        // Navigation properties for 1-to-1 relationships
        public Personel_Detay_SGK? Personel_Detay_SGK { get; set; }
        public Personel_Detay_Muhasebe? Personel_Detay_Muhasebe { get; set; }

        // Navigation properties for 1-to-many relationships
        public virtual ICollection<IletisimBilgileri> IletisimBilgileri { get; set; } = new List<IletisimBilgileri>();
        public virtual ICollection<Belgeler> Belgeler { get; set; } = new List<Belgeler>();
        public virtual ICollection<EgitimBilgileri> EgitimBilgileri { get; set; } = new List<EgitimBilgileri>();
        public virtual ICollection<Zimmet> Zimmetler { get; set; } = new List<Zimmet>();
        public virtual ICollection<MuhasebeHareketleri> MuhasebeHareketleri { get; set; } = new List<MuhasebeHareketleri>();
        public virtual ICollection<IzinTalepleri> IzinTalepleri { get; set; } = new List<IzinTalepleri>();
        public virtual ICollection<PuantajGunluk> PuantajGunlukKayitlari { get; set; } = new List<PuantajGunluk>();
    }
}
