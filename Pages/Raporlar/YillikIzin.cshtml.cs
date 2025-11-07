using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using LoyalKullaniciTakip.Data;
using LoyalKullaniciTakip.Data.Lookups;

namespace LoyalKullaniciTakip.Pages.Raporlar
{
    public class YillikIzinModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public YillikIzinModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<YillikIzinRaporuViewModel> Rapor { get; set; } = new List<YillikIzinRaporuViewModel>();

        public async Task OnGetAsync()
        {
            var bugun = DateTime.Today;

            // Kıdem izin kurallarını veritabanından çek
            var kidemIzinKurallari = await _context.Lookup_KidemIzinHakedis
                .OrderBy(k => k.MinKidemYili)
                .ToListAsync();

            // Tüm aktif personelleri izin talepleriyle ve SGK detaylarıyla birlikte yükle
            var personeller = await _context.Personeller
                .Include(p => p.Personel_Detay_SGK) // İşe giriş tarihi için SGK detayını yükle
                .Include(p => p.IzinTalepleri)
                    .ThenInclude(i => i.IzinTipi)
                .Where(p => p.Personel_Detay_SGK != null) // SGK detayı olmayan personelleri filtrele
                .ToListAsync();

            Rapor = new List<YillikIzinRaporuViewModel>();

            foreach (var personel in personeller)
            {
                // SGK detayı kontrolü
                if (personel.Personel_Detay_SGK == null)
                    continue;

                // Kıdem yılı hesapla
                var kidemYili = CalculateKidemYili(personel.Personel_Detay_SGK.IseGirisTarihi, bugun);

                // Hakedilen toplam izin hesapla (dinamik olarak kurallar tablosundan)
                var hakedilenIzin = CalculateHakedilenIzin(kidemYili, kidemIzinKurallari);

                // Kullanılan izin hesapla (yıllık izinden düşen ve onaylanan izinler)
                var kullanilanIzin = personel.IzinTalepleri
                    .Where(i => i.OnayDurumu == 1 && i.IzinTipi.YillikIzindenDuserMi)
                    .Sum(i => (i.BitisTarihi.Date - i.BaslangicTarihi.Date).Days + 1);

                // Kalan izin hesapla
                var kalanIzin = hakedilenIzin - kullanilanIzin;

                Rapor.Add(new YillikIzinRaporuViewModel
                {
                    PersonelAdSoyad = $"{personel.Ad} {personel.Soyad}",
                    IseGirisTarihi = personel.Personel_Detay_SGK.IseGirisTarihi,
                    KidemYili = kidemYili,
                    HakedilenToplamIzin = hakedilenIzin,
                    KullanilanIzin = kullanilanIzin,
                    KalanIzin = kalanIzin
                });
            }

            // Personel adına göre sırala
            Rapor = Rapor.OrderBy(r => r.PersonelAdSoyad).ToList();
        }

        /// <summary>
        /// Kıdem yılını hesaplar (tam yıl)
        /// </summary>
        private int CalculateKidemYili(DateTime iseGirisTarihi, DateTime referansTarih)
        {
            var yil = referansTarih.Year - iseGirisTarihi.Year;
            
            // Eğer bu yıl daha doğum günü gelmemişse 1 yıl eksilt
            if (referansTarih.Month < iseGirisTarihi.Month || 
                (referansTarih.Month == iseGirisTarihi.Month && referansTarih.Day < iseGirisTarihi.Day))
            {
                yil--;
            }

            return yil;
        }

        /// <summary>
        /// Kıdeme göre hakedilen yıllık izin gün sayısını hesaplar
        /// Kurallar Lookup_KidemIzinHakedis tablosundan dinamik olarak alınır
        /// </summary>
        private int CalculateHakedilenIzin(int kidemYili, List<Lookup_KidemIzinHakedis> kurallar)
        {
            if (kidemYili < 1)
                return 0; // İlk yıl henüz hak etmedi

            // Kıdem yılına göre uygun kuralı bul
            var kural = kurallar.FirstOrDefault(k => kidemYili >= k.MinKidemYili && kidemYili <= k.MaxKidemYili);

            // Kural bulunduysa hakedilen gün sayısını döndür, bulunamadıysa 0
            return kural?.HakedilenGunSayisi ?? 0;
        }
    }

    /// <summary>
    /// Yıllık izin raporu için ViewModel
    /// </summary>
    public class YillikIzinRaporuViewModel
    {
        public string PersonelAdSoyad { get; set; } = string.Empty;
        public DateTime IseGirisTarihi { get; set; }
        public int KidemYili { get; set; }
        public int HakedilenToplamIzin { get; set; }
        public int KullanilanIzin { get; set; }
        public int KalanIzin { get; set; }
    }
}

