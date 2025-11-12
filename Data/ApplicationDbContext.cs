using Microsoft.EntityFrameworkCore;
using LoyalKullaniciTakip.Data.Lookups;

namespace LoyalKullaniciTakip.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets
        public DbSet<Personel> Personeller { get; set; }
        public DbSet<Personel_Detay_SGK> Personel_Detay_SGK { get; set; }
        public DbSet<Personel_Detay_Muhasebe> Personel_Detay_Muhasebe { get; set; }
        public DbSet<IletisimBilgileri> IletisimBilgileri { get; set; }
        public DbSet<Belgeler> Belgeler { get; set; }
        public DbSet<EgitimBilgileri> EgitimBilgileri { get; set; }
        public DbSet<Zimmet> Zimmetler { get; set; }
        public DbSet<MuhasebeHareketleri> MuhasebeHareketleri { get; set; }
        public DbSet<IzinTalepleri> IzinTalepleri { get; set; }
        public DbSet<PuantajGunluk> PuantajGunluk { get; set; }
        public DbSet<BordroHakedisKaydi> BordroHakedisKayitlari { get; set; }
        public DbSet<EkMesaiKayitlari> EkMesaiKayitlari { get; set; }

        // Lookup Tables
        public DbSet<Lookup_CalismaTipi> Lookup_CalismaTipi { get; set; }
        public DbSet<Lookup_MeslekKodlari> Lookup_MeslekKodlari { get; set; }
        public DbSet<Lookup_IletisimTipleri> Lookup_IletisimTipleri { get; set; }
        public DbSet<Lookup_BelgeKategorileri> Lookup_BelgeKategorileri { get; set; }
        public DbSet<Lookup_MuhasebeHareketTipi> Lookup_MuhasebeHareketTipi { get; set; }
        public DbSet<Lookup_IzinTipleri> Lookup_IzinTipleri { get; set; }
        public DbSet<Lookup_PuantajDurumlari> Lookup_PuantajDurumlari { get; set; }
        public DbSet<Lookup_KidemIzinHakedis> Lookup_KidemIzinHakedis { get; set; }
        public DbSet<Lookup_Departmanlar> Lookup_Departmanlar { get; set; }
        public DbSet<Lookup_Meslekler> Lookup_Meslekler { get; set; }
        public DbSet<Lookup_GenelAyarlar> Lookup_GenelAyarlar { get; set; }
        public DbSet<Lookup_EkMesaiKatsayilari> Lookup_EkMesaiKatsayilari { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Personel entity configuration
            modelBuilder.Entity<Personel>(entity =>
            {
                entity.HasKey(p => p.PersonelID);

                // TCKimlikNo unique constraint
                entity.HasIndex(p => p.TCKimlikNo)
                      .IsUnique();

                // Many-to-1 relationship with Lookup_Departmanlar
                entity.HasOne(p => p.Departman)
                      .WithMany()
                      .HasForeignKey(p => p.DepartmanID)
                      .OnDelete(DeleteBehavior.Restrict);

                // Many-to-1 relationship with Lookup_Meslekler
                entity.HasOne(p => p.Meslek)
                      .WithMany()
                      .HasForeignKey(p => p.MeslekID)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Personel_Detay_SGK entity configuration
            modelBuilder.Entity<Personel_Detay_SGK>(entity =>
            {
                entity.HasKey(pd => pd.PersonelID);

                // 1-to-1 relationship with Personel
                entity.HasOne(pd => pd.Personel)
                      .WithOne(p => p.Personel_Detay_SGK)
                      .HasForeignKey<Personel_Detay_SGK>(pd => pd.PersonelID)
                      .OnDelete(DeleteBehavior.Cascade);

                // Many-to-1 relationship with Lookup_CalismaTipi
                entity.HasOne(pd => pd.CalismaTipi)
                      .WithMany()
                      .HasForeignKey(pd => pd.CalismaTipiID)
                      .OnDelete(DeleteBehavior.Restrict);

                // Many-to-1 relationship with Lookup_MeslekKodlari
                entity.HasOne(pd => pd.MeslekKodu)
                      .WithMany()
                      .HasForeignKey(pd => pd.SGKMeslekKoduID)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Personel_Detay_Muhasebe entity configuration
            modelBuilder.Entity<Personel_Detay_Muhasebe>(entity =>
            {
                entity.HasKey(pm => pm.PersonelID);

                // IBAN unique constraint
                entity.HasIndex(pm => pm.IBAN)
                      .IsUnique();

                // 1-to-1 relationship with Personel
                entity.HasOne(pm => pm.Personel)
                      .WithOne(p => p.Personel_Detay_Muhasebe)
                      .HasForeignKey<Personel_Detay_Muhasebe>(pm => pm.PersonelID)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Lookup_CalismaTipi entity configuration
            modelBuilder.Entity<Lookup_CalismaTipi>(entity =>
            {
                entity.HasKey(c => c.CalismaTipiID);
            });

            // Lookup_MeslekKodlari entity configuration
            modelBuilder.Entity<Lookup_MeslekKodlari>(entity =>
            {
                entity.HasKey(m => m.MeslekKoduID);
            });

            // Lookup_IletisimTipleri entity configuration
            modelBuilder.Entity<Lookup_IletisimTipleri>(entity =>
            {
                entity.HasKey(i => i.IletisimTipiID);
            });

            // IletisimBilgileri entity configuration
            modelBuilder.Entity<IletisimBilgileri>(entity =>
            {
                entity.HasKey(i => i.IletisimID);

                // 1-to-many relationship with Personel
                entity.HasOne(i => i.Personel)
                      .WithMany(p => p.IletisimBilgileri)
                      .HasForeignKey(i => i.PersonelID)
                      .OnDelete(DeleteBehavior.Cascade);

                // Many-to-1 relationship with Lookup_IletisimTipleri
                entity.HasOne(i => i.IletisimTipi)
                      .WithMany()
                      .HasForeignKey(i => i.IletisimTipiID)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Lookup_BelgeKategorileri entity configuration
            modelBuilder.Entity<Lookup_BelgeKategorileri>(entity =>
            {
                entity.HasKey(b => b.BelgeKategoriID);
            });

            // Belgeler entity configuration
            modelBuilder.Entity<Belgeler>(entity =>
            {
                entity.HasKey(b => b.BelgeID);

                // 1-to-many relationship with Personel
                entity.HasOne(b => b.Personel)
                      .WithMany(p => p.Belgeler)
                      .HasForeignKey(b => b.PersonelID)
                      .OnDelete(DeleteBehavior.Cascade);

                // Many-to-1 relationship with Lookup_BelgeKategorileri
                entity.HasOne(b => b.BelgeKategori)
                      .WithMany()
                      .HasForeignKey(b => b.BelgeKategoriID)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // EgitimBilgileri entity configuration
            modelBuilder.Entity<EgitimBilgileri>(entity =>
            {
                entity.HasKey(e => e.EgitimKayitID);

                // 1-to-many relationship with Personel
                entity.HasOne(e => e.Personel)
                      .WithMany(p => p.EgitimBilgileri)
                      .HasForeignKey(e => e.PersonelID)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Zimmet entity configuration
            modelBuilder.Entity<Zimmet>(entity =>
            {
                entity.HasKey(z => z.ZimmetID);

                // SeriNo unique constraint
                entity.HasIndex(z => z.SeriNo)
                      .IsUnique();

                // 1-to-many relationship with Personel
                entity.HasOne(z => z.Personel)
                      .WithMany(p => p.Zimmetler)
                      .HasForeignKey(z => z.PersonelID)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Lookup_MuhasebeHareketTipi entity configuration
            modelBuilder.Entity<Lookup_MuhasebeHareketTipi>(entity =>
            {
                entity.HasKey(m => m.HareketTipiID);
            });

            // MuhasebeHareketleri entity configuration
            modelBuilder.Entity<MuhasebeHareketleri>(entity =>
            {
                entity.HasKey(m => m.HareketID);

                // Configure decimal precision for Tutar
                entity.Property(m => m.Tutar)
                      .HasColumnType("decimal(18,2)");

                // 1-to-many relationship with Personel
                entity.HasOne(m => m.Personel)
                      .WithMany(p => p.MuhasebeHareketleri)
                      .HasForeignKey(m => m.PersonelID)
                      .OnDelete(DeleteBehavior.Cascade);

                // Many-to-1 relationship with Lookup_MuhasebeHareketTipi
                entity.HasOne(m => m.HareketTipi)
                      .WithMany()
                      .HasForeignKey(m => m.HareketTipiID)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Lookup_IzinTipleri entity configuration
            modelBuilder.Entity<Lookup_IzinTipleri>(entity =>
            {
                entity.HasKey(i => i.IzinTipiID);
            });

            // Lookup_PuantajDurumlari entity configuration
            modelBuilder.Entity<Lookup_PuantajDurumlari>(entity =>
            {
                entity.HasKey(p => p.PuantajDurumID);
            });

            // Lookup_KidemIzinHakedis entity configuration
            modelBuilder.Entity<Lookup_KidemIzinHakedis>(entity =>
            {
                entity.HasKey(k => k.KidemIzinID);
            });

            // Lookup_Departmanlar entity configuration
            modelBuilder.Entity<Lookup_Departmanlar>(entity =>
            {
                entity.HasKey(d => d.DepartmanID);
            });

            // Lookup_Meslekler entity configuration
            modelBuilder.Entity<Lookup_Meslekler>(entity =>
            {
                entity.HasKey(m => m.MeslekID);
            });

            // Lookup_GenelAyarlar entity configuration
            modelBuilder.Entity<Lookup_GenelAyarlar>(entity =>
            {
                entity.HasKey(g => g.AyarKey);
            });

            // IzinTalepleri entity configuration
            modelBuilder.Entity<IzinTalepleri>(entity =>
            {
                entity.HasKey(i => i.TalepID);

                // 1-to-many relationship with Personel (Talep Eden)
                entity.HasOne(i => i.Personel)
                      .WithMany(p => p.IzinTalepleri)
                      .HasForeignKey(i => i.PersonelID)
                      .OnDelete(DeleteBehavior.Cascade);

                // Many-to-1 relationship with Lookup_IzinTipleri
                entity.HasOne(i => i.IzinTipi)
                      .WithMany()
                      .HasForeignKey(i => i.IzinTipiID)
                      .OnDelete(DeleteBehavior.Restrict);

                // Self-referencing relationship with Personel (Onaylayan)
                entity.HasOne(i => i.OnaylayanPersonel)
                      .WithMany()
                      .HasForeignKey(i => i.OnaylayanPersonelID)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // PuantajGunluk entity configuration
            modelBuilder.Entity<PuantajGunluk>(entity =>
            {
                entity.HasKey(p => p.PuantajID);

                // ÇOK ÖNEMLİ: Composite unique index on PersonelID and Tarih
                // Bu bir personelin aynı güne birden fazla kayıt atmasını engeller
                // ve sorgu performansını ışık hızına çıkarır
                entity.HasIndex(p => new { p.PersonelID, p.Tarih })
                      .IsUnique();

                // Configure decimal precision for MesaiSaati
                entity.Property(p => p.MesaiSaati)
                      .HasColumnType("decimal(18,2)");

                // 1-to-many relationship with Personel
                entity.HasOne(p => p.Personel)
                      .WithMany(per => per.PuantajGunlukKayitlari)
                      .HasForeignKey(p => p.PersonelID)
                      .OnDelete(DeleteBehavior.Cascade);

                // Many-to-1 relationship with Lookup_PuantajDurumlari
                entity.HasOne(p => p.PuantajDurum)
                      .WithMany()
                      .HasForeignKey(p => p.PuantajDurumID)
                      .OnDelete(DeleteBehavior.Restrict);

                // Many-to-1 relationship with Lookup_Departmanlar (optional)
                entity.HasOne(p => p.Departman)
                      .WithMany()
                      .HasForeignKey(p => p.DepartmanID)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // BordroHakedisKaydi entity configuration
            modelBuilder.Entity<BordroHakedisKaydi>(entity =>
            {
                entity.HasKey(b => b.KayitID);

                // Composite unique index on PersonelID, Yil, Ay
                // Her personelin her ay için sadece bir kaydı olabilir
                entity.HasIndex(b => new { b.PersonelID, b.Yil, b.Ay })
                      .IsUnique();

                // Configure decimal precision
                entity.Property(b => b.BrutMaas)
                      .HasColumnType("decimal(18,2)");

                entity.Property(b => b.KesintiTutari)
                      .HasColumnType("decimal(18,2)");

                entity.Property(b => b.NetHakedis)
                      .HasColumnType("decimal(18,2)");

                // 1-to-many relationship with Personel
                entity.HasOne(b => b.Personel)
                      .WithMany()
                      .HasForeignKey(b => b.PersonelID)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Lookup_EkMesaiKatsayilari entity configuration
            modelBuilder.Entity<Lookup_EkMesaiKatsayilari>(entity =>
            {
                entity.HasKey(k => k.KatsayiID);

                // GunTipi unique constraint
                entity.HasIndex(k => k.GunTipi)
                      .IsUnique();
            });

            // EkMesaiKayitlari entity configuration
            modelBuilder.Entity<EkMesaiKayitlari>(entity =>
            {
                entity.HasKey(e => e.EkMesaiID);

                // Composite unique index on PersonelID and Tarih
                // Aynı personelin aynı güne birden fazla ek mesai kaydı atmasını engeller
                entity.HasIndex(e => new { e.PersonelID, e.Tarih })
                      .IsUnique();

                // Configure decimal precision
                entity.Property(e => e.EkMesaiSaati)
                      .HasColumnType("decimal(18,2)");

                entity.Property(e => e.Katsayi)
                      .HasColumnType("decimal(18,2)");

                entity.Property(e => e.SaatlikUcret)
                      .HasColumnType("decimal(18,2)");

                entity.Property(e => e.HesaplananTutar)
                      .HasColumnType("decimal(18,2)");

                // 1-to-many relationship with Personel
                entity.HasOne(e => e.Personel)
                      .WithMany()
                      .HasForeignKey(e => e.PersonelID)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
