using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LoyalKullaniciTakip.Data;
using System.Globalization;

namespace LoyalKullaniciTakip.Pages.Raporlar
{
    public class HakedisHesaplamaModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public HakedisHesaplamaModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public int SecilenAy { get; set; }

        [BindProperty]
        public int SecilenYil { get; set; }

        public List<HakedisHesaplamaViewModel> HakedisRaporu { get; set; } = new List<HakedisHesaplamaViewModel>();
        public List<SelectListItem> AyListesi { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> YilListesi { get; set; } = new List<SelectListItem>();
        public bool RaporGosterilsin { get; set; } = false;

        public void OnGet()
        {
            // Varsayılan olarak bu ayı seç
            SecilenAy = DateTime.Today.Month;
            SecilenYil = DateTime.Today.Year;

            PrepareDropdowns();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            PrepareDropdowns();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            RaporGosterilsin = true;

            // 1. Genel ayarları yükle
            var genelAyarlar = await _context.Lookup_GenelAyarlar.ToListAsync();
            var gunlukCalismaSaati = decimal.Parse(genelAyarlar.FirstOrDefault(a => a.AyarKey == "GunlukCalismaSaati")?.AyarValue ?? "8", CultureInfo.InvariantCulture);
            var haftaIciKatsayi = decimal.Parse(genelAyarlar.FirstOrDefault(a => a.AyarKey == "HaftaIciMesaiCarpani")?.AyarValue ?? "1.5", CultureInfo.InvariantCulture);
            var cumartesiKatsayi = decimal.Parse(genelAyarlar.FirstOrDefault(a => a.AyarKey == "CumartesiMesaiCarpani")?.AyarValue ?? "1.5", CultureInfo.InvariantCulture);
            var pazarKatsayi = decimal.Parse(genelAyarlar.FirstOrDefault(a => a.AyarKey == "PazarMesaiCarpani")?.AyarValue ?? "2.0", CultureInfo.InvariantCulture);

            // 2. Personelleri ve muhasebe detaylarını çek
            var personeller = await _context.Personeller
                .Include(p => p.Personel_Detay_Muhasebe)
                .Where(p => p.Personel_Detay_Muhasebe != null)
                .ToListAsync();

            // 3. Puantaj durumlarını çek (kesinti hesapları için)
            var raporluDurum = await _context.Lookup_PuantajDurumlari
                .FirstOrDefaultAsync(d => d.Kod == "R" || d.Tanim.Contains("Raporlu"));
            var devamsizDurum = await _context.Lookup_PuantajDurumlari
                .FirstOrDefaultAsync(d => d.Kod == "D" || d.Tanim.Contains("Devamsız"));

            // 4. Tüm puantaj kayıtlarını çek (detaylı mesai hesabı için)
            var ayinIlkGunu = new DateTime(SecilenYil, SecilenAy, 1);
            var ayinSonGunu = ayinIlkGunu.AddMonths(1).AddDays(-1);

            var puantajKayitlari = await _context.PuantajGunluk
                .Include(p => p.Personel)
                .Include(p => p.PuantajDurum)
                .Where(p => p.Tarih >= ayinIlkGunu && p.Tarih <= ayinSonGunu)
                .ToListAsync();

            // 4. Hakedis hesaplamalarını yap
            HakedisRaporu = new List<HakedisHesaplamaViewModel>();

            var personelGruplari = puantajKayitlari
                .GroupBy(p => new { p.PersonelID, p.Personel.Ad, p.Personel.Soyad });

            foreach (var grup in personelGruplari)
            {
                var personel = personeller.FirstOrDefault(p => p.PersonelID == grup.Key.PersonelID);
                if (personel?.Personel_Detay_Muhasebe == null)
                    continue;

                var brutMaas = personel.Personel_Detay_Muhasebe.TemelMaas;
                
                // Saatlik ücret hesabı (Aylık standart çalışma saati: 30 gün × günlük saat)
                var aylikStandartSaat = 30 * gunlukCalismaSaati;
                var saatlikUcret = brutMaas / aylikStandartSaat;
                var gunlukMaas = brutMaas / 30m;

                // Mesai hesaplamaları
                decimal normalMesaiSaati = 0;
                decimal haftaIciFazlaMesai = 0;
                decimal cumartesiFazlaMesai = 0;
                decimal pazarFazlaMesai = 0;
                int raporluGun = 0;
                int ucretsizIzinGun = 0;
                int devamsizGun = 0;

                foreach (var kayit in grup)
                {
                    var mesaiSaati = kayit.MesaiSaati ?? 0;

                    // Kesinti günlerini say
                    if (raporluDurum != null && kayit.PuantajDurumID == raporluDurum.PuantajDurumID)
                        raporluGun++;
                    else if (devamsizDurum != null && kayit.PuantajDurumID == devamsizDurum.PuantajDurumID)
                        devamsizGun++;
                    else if (kayit.PuantajDurumID == 5) // Ücretsiz İzin (eski ID korundu)
                        ucretsizIzinGun++;

                    // Mesai saati varsa hesapla
                    if (mesaiSaati > 0)
                    {
                        // Günlük normal mesai (0-8 saat arası)
                        var gunlukNormalSaat = Math.Min(mesaiSaati, gunlukCalismaSaati);
                        normalMesaiSaati += gunlukNormalSaat;

                        // Fazla mesai (8 saatten fazlası)
                        if (mesaiSaati > gunlukCalismaSaati)
                        {
                            var fazlaSaat = mesaiSaati - gunlukCalismaSaati;
                            
                            // Günün türüne göre sınıflandır
                            if (kayit.Tarih.DayOfWeek == DayOfWeek.Sunday)
                            {
                                pazarFazlaMesai += fazlaSaat;
                            }
                            else if (kayit.Tarih.DayOfWeek == DayOfWeek.Saturday)
                            {
                                cumartesiFazlaMesai += fazlaSaat;
                            }
                            else // Hafta içi (Pazartesi-Cuma)
                            {
                                haftaIciFazlaMesai += fazlaSaat;
                            }
                        }
                    }
                }

                // Ücret hesaplamaları
                var haftaIciFazlaMesaiUcreti = haftaIciFazlaMesai * saatlikUcret * haftaIciKatsayi;
                var cumartesiFazlaMesaiUcreti = cumartesiFazlaMesai * saatlikUcret * cumartesiKatsayi;
                var pazarFazlaMesaiUcreti = pazarFazlaMesai * saatlikUcret * pazarKatsayi;
                var toplamFazlaMesaiUcreti = haftaIciFazlaMesaiUcreti + cumartesiFazlaMesaiUcreti + pazarFazlaMesaiUcreti;

                // Ek Mesai (SADECE Manuel giriş yapılan kayıtlar) tutarını ekle
                // "Puantaj Otomatik" açıklamalı kayıtlar zaten yukarıda puantajdan hesaplandı, çift sayılmamalı
                var manuelEkMesaiTutari = await _context.EkMesaiKayitlari
                    .Where(e => e.PersonelID == grup.Key.PersonelID &&
                                e.Tarih >= ayinIlkGunu &&
                                e.Tarih <= ayinSonGunu &&
                                e.Aciklama != "Puantaj Otomatik")
                    .SumAsync(e => e.HesaplananTutar);

                // Toplam fazla mesai ücretine SADECE manuel girilen ek mesai tutarını ekle
                toplamFazlaMesaiUcreti += manuelEkMesaiTutari;

                // Kesintiler (Raporlu, Devamsız, Ücretsiz İzin)
                var toplamKesintiGunu = raporluGun + devamsizGun + ucretsizIzinGun;
                var kesintiTutari = toplamKesintiGunu * gunlukMaas;

                // Net hakediş
                var netHakedis = brutMaas + toplamFazlaMesaiUcreti - kesintiTutari;

                // Özet bilgileri için
                var puantajOzet = new PuantajOzetViewModel
                {
                    PersonelAdSoyad = $"{grup.Key.Ad} {grup.Key.Soyad}",
                    CalisilanGunSayisi = grup.Count(x => x.PuantajDurumID == 1),
                    RaporluGunSayisi = raporluGun,
                    DevamsizGunSayisi = devamsizGun,
                    YillikIzinGunSayisi = grup.Count(x => x.PuantajDurumID == 3),
                    MazeretIzniGunSayisi = grup.Count(x => x.PuantajDurumID == 4),
                    UcretsizIzinGunSayisi = ucretsizIzinGun,
                    TatilGunSayisi = grup.Count(x => x.PuantajDurumID == 7 || x.PuantajDurumID == 8 || x.PuantajDurumID == 9 || x.PuantajDurumID == 11),
                    FazlaMesaiGunSayisi = grup.Count(x => x.PuantajDurumID == 10),
                    ToplamMesaiSaati = grup.Sum(x => x.MesaiSaati ?? 0),
                    ToplamGunSayisi = grup.Count()
                };

                HakedisRaporu.Add(new HakedisHesaplamaViewModel
                {
                    PersonelID = grup.Key.PersonelID,
                    PersonelAdSoyad = puantajOzet.PersonelAdSoyad,
                    BrutMaas = brutMaas,
                    SaatlikUcret = saatlikUcret,
                    NormalMesaiSaati = normalMesaiSaati,
                    HaftaIciFazlaMesaiSaati = haftaIciFazlaMesai,
                    CumartesiFazlaMesaiSaati = cumartesiFazlaMesai,
                    PazarFazlaMesaiSaati = pazarFazlaMesai,
                    HaftaIciFazlaMesaiUcreti = haftaIciFazlaMesaiUcreti,
                    CumartesiFazlaMesaiUcreti = cumartesiFazlaMesaiUcreti,
                    PazarFazlaMesaiUcreti = pazarFazlaMesaiUcreti,
                    ToplamFazlaMesaiUcreti = toplamFazlaMesaiUcreti,
                    PuantajOzet = puantajOzet,
                    ToplamKesintiGunu = toplamKesintiGunu,
                    KesintiTutari = kesintiTutari,
                    NetHakedis = netHakedis
                });
            }

            // Personel adına göre sırala
            HakedisRaporu = HakedisRaporu.OrderBy(h => h.PersonelAdSoyad).ToList();

            return Page();
        }

        /// <summary>
        /// Hesaplanan hakedişleri veritabanına kaydeder (UPSERT)
        /// </summary>
        public async Task<IActionResult> OnPostSaveHakedisAsync()
        {
            PrepareDropdowns();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Önce raporu tekrar hesapla
            var onPostResult = await OnPostAsync();
            if (onPostResult is not PageResult)
            {
                return onPostResult;
            }

            if (!HakedisRaporu.Any())
            {
                TempData["ErrorMessage"] = "Kaydedilecek hakedis bulunamadı.";
                RaporGosterilsin = true;
                return Page();
            }

            try
            {
                var hesaplananTarih = DateTime.Now;
                int kaydedilenSayisi = 0;
                int guncellenenSayisi = 0;

                // Personel ID'lerini bul
                var personelAdlari = HakedisRaporu.Select(h => h.PersonelAdSoyad).ToList();
                var personeller = await _context.Personeller
                    .Where(p => personelAdlari.Contains(p.Ad + " " + p.Soyad))
                    .ToListAsync();

                foreach (var hakedis in HakedisRaporu)
                {
                    var personel = personeller.FirstOrDefault(p => 
                        $"{p.Ad} {p.Soyad}" == hakedis.PersonelAdSoyad);

                    if (personel == null)
                        continue;

                    // UPSERT: Mevcut kayıt var mı kontrol et
                    var mevcutKayit = await _context.BordroHakedisKayitlari
                        .FirstOrDefaultAsync(b => b.PersonelID == personel.PersonelID 
                                                  && b.Yil == SecilenYil 
                                                  && b.Ay == SecilenAy);

                    if (mevcutKayit != null)
                    {
                        // Güncelle
                        mevcutKayit.HesaplananTarih = hesaplananTarih;
                        mevcutKayit.BrutMaas = hakedis.BrutMaas;
                        mevcutKayit.FazlaMesaiUcreti = hakedis.ToplamFazlaMesaiUcreti;
                        mevcutKayit.KesintiTutari = hakedis.KesintiTutari;
                        mevcutKayit.NetHakedis = hakedis.NetHakedis;
                        guncellenenSayisi++;
                    }
                    else
                    {
                        // Yeni kayıt ekle
                        _context.BordroHakedisKayitlari.Add(new BordroHakedisKaydi
                        {
                            PersonelID = personel.PersonelID,
                            Yil = SecilenYil,
                            Ay = SecilenAy,
                            HesaplananTarih = hesaplananTarih,
                            BrutMaas = hakedis.BrutMaas,
                            FazlaMesaiUcreti = hakedis.ToplamFazlaMesaiUcreti,
                            KesintiTutari = hakedis.KesintiTutari,
                            NetHakedis = hakedis.NetHakedis
                        });
                        kaydedilenSayisi++;
                    }
                }

                await _context.SaveChangesAsync();

                var turkishCulture = new CultureInfo("tr-TR");
                var ayAdi = turkishCulture.DateTimeFormat.GetMonthName(SecilenAy);
                
                TempData["SuccessMessage"] = $"{ayAdi} {SecilenYil} ayı kapatıldı. " +
                    $"{kaydedilenSayisi} yeni kayıt eklendi, {guncellenenSayisi} kayıt güncellendi.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Kaydetme sırasında hata oluştu: {ex.Message}";
            }

            RaporGosterilsin = true;
            return Page();
        }

        /// <summary>
        /// Seçilen personel ve döneme ait ek mesai detaylarını döndürür (AJAX handler)
        /// </summary>
        public async Task<IActionResult> OnGetEkMesaiDetayAsync(int personelId, int ay, int yil)
        {
            try
            {
                // Seçilen dönemin ilk ve son günleri
                var ayinIlkGunu = new DateTime(yil, ay, 1);
                var ayinSonGunu = ayinIlkGunu.AddMonths(1).AddDays(-1);

                // DEBUG: Tüm ek mesai kayıtlarını kontrol et
                var tumKayitlar = await _context.EkMesaiKayitlari.ToListAsync();
                var tumKayitSayisi = tumKayitlar.Count;

                // DEBUG: Bu personele ait tüm kayıtlar
                var personelKayitlari = tumKayitlar.Where(e => e.PersonelID == personelId).ToList();
                var personelKayitSayisi = personelKayitlari.Count;

                // Personel ve dönem için ek mesai kayıtlarını çek
                var ekMesaiKayitlari = await _context.EkMesaiKayitlari
                    .Where(e => e.PersonelID == personelId &&
                                e.Tarih >= ayinIlkGunu &&
                                e.Tarih <= ayinSonGunu)
                    .OrderBy(e => e.Tarih)
                    .Select(e => new
                    {
                        tarih = e.Tarih.ToString("dd.MM.yyyy"),
                        gunTipi = e.GunTipi == "HaftaIci" ? "Hafta İçi" :
                                  e.GunTipi == "Cumartesi" ? "Cumartesi" : "Pazar",
                        ekMesaiSaati = e.EkMesaiSaati.ToString("N1"),
                        katsayi = e.Katsayi.ToString("N2"),
                        saatlikUcret = e.SaatlikUcret.ToString("N2"),
                        hesaplananTutar = e.HesaplananTutar.ToString("N2")
                    })
                    .ToListAsync();

                // Toplamları hesapla
                var toplamSaat = await _context.EkMesaiKayitlari
                    .Where(e => e.PersonelID == personelId &&
                                e.Tarih >= ayinIlkGunu &&
                                e.Tarih <= ayinSonGunu)
                    .SumAsync(e => e.EkMesaiSaati);

                var toplamTutar = await _context.EkMesaiKayitlari
                    .Where(e => e.PersonelID == personelId &&
                                e.Tarih >= ayinIlkGunu &&
                                e.Tarih <= ayinSonGunu)
                    .SumAsync(e => e.HesaplananTutar);

                return new JsonResult(new
                {
                    success = true,
                    kayitlar = ekMesaiKayitlari,
                    toplamSaat = toplamSaat.ToString("N1"),
                    toplamTutar = toplamTutar.ToString("N2"),
                    // DEBUG bilgileri
                    debug = new
                    {
                        personelId = personelId,
                        ay = ay,
                        yil = yil,
                        ayinIlkGunu = ayinIlkGunu.ToString("yyyy-MM-dd"),
                        ayinSonGunu = ayinSonGunu.ToString("yyyy-MM-dd"),
                        tumKayitSayisi = tumKayitSayisi,
                        personelKayitSayisi = personelKayitSayisi,
                        personelTumTarihler = personelKayitlari.Select(k => k.Tarih.ToString("yyyy-MM-dd")).ToList()
                    }
                });
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    success = false,
                    mesaj = $"Veriler yüklenirken hata oluştu: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Puantaj özet raporunu oluşturur (Sprint 5 US 3 mantığı)
        /// </summary>
        private async Task<List<PuantajOzetViewModel>> GetPuantajOzetleri(int yil, int ay)
        {
            // Devamsız durumunu lookup'tan çek
            var devamsizDurum = await _context.Lookup_PuantajDurumlari
                .FirstOrDefaultAsync(d => d.Kod == "D" || d.Tanim.Contains("Devamsız"));
            var devamsizDurumId = devamsizDurum?.PuantajDurumID ?? 0;

            var puantajOzetleri = await _context.PuantajGunluk
                .Include(p => p.Personel)
                .Include(p => p.PuantajDurum)
                .Where(p => p.Tarih.Year == yil && p.Tarih.Month == ay)
                .GroupBy(p => new { p.PersonelID, p.Personel.Ad, p.Personel.Soyad })
                .Select(g => new PuantajOzetViewModel
                {
                    PersonelAdSoyad = g.Key.Ad + " " + g.Key.Soyad,
                    CalisilanGunSayisi = g.Count(x => x.PuantajDurumID == 1), // Çalıştı
                    RaporluGunSayisi = g.Count(x => x.PuantajDurumID == 2), // Raporlu
                    DevamsizGunSayisi = g.Count(x => x.PuantajDurumID == devamsizDurumId), // Devamsız
                    YillikIzinGunSayisi = g.Count(x => x.PuantajDurumID == 3), // Yıllık İzin
                    MazeretIzniGunSayisi = g.Count(x => x.PuantajDurumID == 4), // Mazeret İzni
                    UcretsizIzinGunSayisi = g.Count(x => x.PuantajDurumID == 5), // Ücretsiz İzin
                    TatilGunSayisi = g.Count(x => x.PuantajDurumID == 7 || x.PuantajDurumID == 8 || x.PuantajDurumID == 9 || x.PuantajDurumID == 11), // Tatil, CT, Pazar, Genel Tatil
                    FazlaMesaiGunSayisi = g.Count(x => x.PuantajDurumID == 10), // Fazla Mesai
                    ToplamMesaiSaati = g.Sum(x => x.MesaiSaati ?? 0),
                    ToplamGunSayisi = g.Count()
                })
                .OrderBy(r => r.PersonelAdSoyad)
                .ToListAsync();

            return puantajOzetleri;
        }

        private void PrepareDropdowns()
        {
            // Aylar listesi (Türkçe)
            var turkishCulture = new CultureInfo("tr-TR");
            AyListesi = Enumerable.Range(1, 12)
                .Select(i => new SelectListItem
                {
                    Value = i.ToString(),
                    Text = turkishCulture.DateTimeFormat.GetMonthName(i),
                    Selected = i == SecilenAy
                })
                .ToList();

            // Yıllar listesi (Son 5 yıl + gelecek 1 yıl)
            var buYil = DateTime.Today.Year;
            YilListesi = Enumerable.Range(buYil - 5, 7)
                .Select(y => new SelectListItem
                {
                    Value = y.ToString(),
                    Text = y.ToString(),
                    Selected = y == SecilenYil
                })
                .OrderByDescending(x => x.Value)
                .ToList();
        }
    }

    /// <summary>
    /// Hakedis hesaplama raporu için ViewModel
    /// </summary>
    public class HakedisHesaplamaViewModel
    {
        public int PersonelID { get; set; }
        public string PersonelAdSoyad { get; set; } = string.Empty;
        public decimal BrutMaas { get; set; }
        public decimal SaatlikUcret { get; set; }
        
        // Mesai saatleri
        public decimal NormalMesaiSaati { get; set; }
        public decimal HaftaIciFazlaMesaiSaati { get; set; }
        public decimal CumartesiFazlaMesaiSaati { get; set; }
        public decimal PazarFazlaMesaiSaati { get; set; }
        
        // Fazla mesai ücretleri
        public decimal HaftaIciFazlaMesaiUcreti { get; set; }
        public decimal CumartesiFazlaMesaiUcreti { get; set; }
        public decimal PazarFazlaMesaiUcreti { get; set; }
        public decimal ToplamFazlaMesaiUcreti { get; set; }
        
        // Kesintiler ve toplam
        public PuantajOzetViewModel PuantajOzet { get; set; } = new PuantajOzetViewModel();
        public int ToplamKesintiGunu { get; set; }
        public decimal KesintiTutari { get; set; }
        public decimal NetHakedis { get; set; }
    }
}

