using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LoyalKullaniciTakip.Data;
using LoyalKullaniciTakip.Data.Lookups;

namespace LoyalKullaniciTakip.Pages.Puantaj
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public int SecilenAy { get; set; } = DateTime.Now.Month;

        [BindProperty]
        public int SecilenYil { get; set; } = DateTime.Now.Year;

        public PuantajViewModel PuantajData { get; set; } = new PuantajViewModel();
        public List<Lookup_PuantajDurumlari> PuantajDurumlari { get; set; } = new List<Lookup_PuantajDurumlari>();
        public List<Lookup_Departmanlar> Departmanlar { get; set; } = new List<Lookup_Departmanlar>();

        // Günlük çalışma saati ayarı
        public decimal GunlukCalismaSaati { get; set; } = 8;

        public async Task OnGetAsync()
        {
            await LoadPuantajDurumlariAsync();
            await LoadDepartmanlarAsync();
            await LoadGunlukCalismaSaatiAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await LoadPuantajDurumlariAsync();
            await LoadDepartmanlarAsync();
            await LoadGunlukCalismaSaatiAsync();

            // Seçilen ay/yıl için puantaj verisini yükle
            PuantajData = await LoadPuantajDataAsync(SecilenAy, SecilenYil);

            return Page();
        }

        public async Task<IActionResult> OnPostSaveGridAsync(List<PuantajKayitDto> puantajKayitlari, int SecilenAy, int SecilenYil)
        {
            if (puantajKayitlari == null || !puantajKayitlari.Any())
            {
                TempData["ErrorMessage"] = "Kaydedilecek veri bulunamadı.";
                return RedirectToPage();
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Genel ayarları yükle
                    await LoadGunlukCalismaSaatiAsync();
                    var genelAyarlar = await _context.Lookup_GenelAyarlar.ToListAsync();
                    var haftaIciKatsayi = decimal.Parse(
                        genelAyarlar.FirstOrDefault(a => a.AyarKey == "HaftaIciMesaiCarpani")?.AyarValue ?? "1.5", 
                        System.Globalization.CultureInfo.InvariantCulture);
                    var cumartesiKatsayi = decimal.Parse(
                        genelAyarlar.FirstOrDefault(a => a.AyarKey == "CumartesiMesaiCarpani")?.AyarValue ?? "1.5", 
                        System.Globalization.CultureInfo.InvariantCulture);
                    var pazarKatsayi = decimal.Parse(
                        genelAyarlar.FirstOrDefault(a => a.AyarKey == "PazarMesaiCarpani")?.AyarValue ?? "2.0", 
                        System.Globalization.CultureInfo.InvariantCulture);

                    var kaydedilenSayisi = 0;
                    var ekMesaiKayitSayisi = 0;

                    foreach (var kayit in puantajKayitlari)
                    {
                        // Boş durumları atla
                        if (kayit.PuantajDurumID == 0)
                            continue;

                        var tarih = new DateTime(kayit.Yil, kayit.Ay, kayit.Gun);

                        // UPSERT: Mevcut kaydı bul
                        var mevcutKayit = await _context.PuantajGunluk
                            .FirstOrDefaultAsync(p => p.PersonelID == kayit.PersonelID && p.Tarih == tarih);

                        if (mevcutKayit == null)
                        {
                            // Yeni kayıt ekle
                            var yeniKayit = new PuantajGunluk
                            {
                                PersonelID = kayit.PersonelID,
                                Tarih = tarih,
                                PuantajDurumID = kayit.PuantajDurumID,
                                MesaiSaati = kayit.MesaiSaati,
                                DepartmanID = kayit.DepartmanID, // Departman bilgisi
                                Aciklama = "Manuel girdi"
                            };
                            await _context.PuantajGunluk.AddAsync(yeniKayit);
                            kaydedilenSayisi++;
                        }
                        else
                        {
                            // Mevcut kaydı güncelle
                            mevcutKayit.PuantajDurumID = kayit.PuantajDurumID;
                            mevcutKayit.MesaiSaati = kayit.MesaiSaati;
                            mevcutKayit.DepartmanID = kayit.DepartmanID; // Departman bilgisi
                            mevcutKayit.Aciklama = "Manuel güncelleme";
                            kaydedilenSayisi++;
                        }

                        // Fazla mesai kontrolü ve ek mesai kaydı oluşturma
                        if (kayit.MesaiSaati.HasValue && kayit.MesaiSaati.Value > GunlukCalismaSaati)
                        {
                            var fazlaMesaiSaati = kayit.MesaiSaati.Value - GunlukCalismaSaati;

                            // Personelin muhasebe bilgilerini çek
                            var personel = await _context.Personeller
                                .Include(p => p.Personel_Detay_Muhasebe)
                                .FirstOrDefaultAsync(p => p.PersonelID == kayit.PersonelID);

                            if (personel?.Personel_Detay_Muhasebe != null)
                            {
                                // Saatlik ücret hesabı (Brüt maaş / (30 gün × günlük çalışma saati))
                                var brutMaas = personel.Personel_Detay_Muhasebe.TemelMaas;
                                var aylikStandartSaat = 30 * GunlukCalismaSaati;
                                var saatlikUcret = brutMaas / aylikStandartSaat;

                                // Günün türüne göre katsayı belirle
                                string gunTipi;
                                decimal katsayi;

                                if (tarih.DayOfWeek == DayOfWeek.Sunday)
                                {
                                    gunTipi = "Pazar";
                                    katsayi = pazarKatsayi;
                                }
                                else if (tarih.DayOfWeek == DayOfWeek.Saturday)
                                {
                                    gunTipi = "Cumartesi";
                                    katsayi = cumartesiKatsayi;
                                }
                                else
                                {
                                    gunTipi = "HaftaIci";
                                    katsayi = haftaIciKatsayi;
                                }

                                // Toplam tutar hesabı
                                var hesaplananTutar = fazlaMesaiSaati * saatlikUcret * katsayi;

                                // Ek mesai kaydı UPSERT
                                var mevcutEkMesai = await _context.EkMesaiKayitlari
                                    .FirstOrDefaultAsync(e => e.PersonelID == kayit.PersonelID && 
                                                             e.Tarih == tarih &&
                                                             e.Aciklama == "Puantaj Otomatik");

                                if (mevcutEkMesai == null)
                                {
                                    // Yeni ek mesai kaydı ekle
                                    var yeniEkMesai = new EkMesaiKayitlari
                                    {
                                        PersonelID = kayit.PersonelID,
                                        Tarih = tarih,
                                        EkMesaiSaati = fazlaMesaiSaati,
                                        GunTipi = gunTipi,
                                        Katsayi = katsayi,
                                        SaatlikUcret = saatlikUcret,
                                        HesaplananTutar = hesaplananTutar,
                                        Aciklama = "Puantaj Otomatik",
                                        OlusturmaTarihi = DateTime.Now
                                    };
                                    await _context.EkMesaiKayitlari.AddAsync(yeniEkMesai);
                                    ekMesaiKayitSayisi++;
                                }
                                else
                                {
                                    // Mevcut ek mesai kaydını güncelle
                                    mevcutEkMesai.EkMesaiSaati = fazlaMesaiSaati;
                                    mevcutEkMesai.GunTipi = gunTipi;
                                    mevcutEkMesai.Katsayi = katsayi;
                                    mevcutEkMesai.SaatlikUcret = saatlikUcret;
                                    mevcutEkMesai.HesaplananTutar = hesaplananTutar;
                                    ekMesaiKayitSayisi++;
                                }
                            }
                        }
                        else if (kayit.MesaiSaati.HasValue && kayit.MesaiSaati.Value <= GunlukCalismaSaati)
                        {
                            // Eğer mesai saati günlük çalışma saatinden az veya eşitse,
                            // o tarih için otomatik oluşturulmuş ek mesai kaydını sil
                            var mevcutEkMesai = await _context.EkMesaiKayitlari
                                .FirstOrDefaultAsync(e => e.PersonelID == kayit.PersonelID && 
                                                         e.Tarih == tarih &&
                                                         e.Aciklama == "Puantaj Otomatik");
                            
                            if (mevcutEkMesai != null)
                            {
                                _context.EkMesaiKayitlari.Remove(mevcutEkMesai);
                            }
                        }
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    var mesaj = $"Puantaj kayıtları başarıyla kaydedildi. ({kaydedilenSayisi} kayıt)";
                    if (ekMesaiKayitSayisi > 0)
                    {
                        mesaj += $" {ekMesaiKayitSayisi} ek mesai kaydı oluşturuldu/güncellendi.";
                    }
                    TempData["SuccessMessage"] = mesaj;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    TempData["ErrorMessage"] = $"Kayıt sırasında hata oluştu: {ex.Message} - InnerException: {ex.InnerException?.Message}";
                }
            }

            return RedirectToPage(new { handler = "", ay = SecilenAy, yil = SecilenYil });
        }

        private async Task<PuantajViewModel> LoadPuantajDataAsync(int ay, int yil)
        {
            var viewModel = new PuantajViewModel
            {
                Ay = ay,
                Yil = yil
            };

            // a. Tüm aktif personelleri yükle
            var personeller = await _context.Personeller
                .OrderBy(p => p.Ad)
                .ThenBy(p => p.Soyad)
                .Select(p => new { p.PersonelID, AdSoyad = p.Ad + " " + p.Soyad, p.DepartmanID })
                .ToListAsync();

            // b. Seçilen aydaki tüm puantaj kayıtlarını tek sorguda yükle
            var ayinIlkGunu = new DateTime(yil, ay, 1);
            var ayinSonGunu = ayinIlkGunu.AddMonths(1).AddDays(-1);

            var puantajKayitlari = await _context.PuantajGunluk
                .Include(p => p.PuantajDurum)
                .Where(p => p.Tarih >= ayinIlkGunu && p.Tarih <= ayinSonGunu)
                .ToListAsync();

            // c. ViewModel'i doldur
            var aydakiGunSayisi = DateTime.DaysInMonth(yil, ay);

            foreach (var personel in personeller)
            {
                var satir = new PersonelPuantajSatiri
                {
                    PersonelID = personel.PersonelID,
                    PersonelAdSoyad = personel.AdSoyad,
                    DepartmanID = personel.DepartmanID
                };

                // Her gün için kayıt oluştur
                for (int gun = 1; gun <= aydakiGunSayisi; gun++)
                {
                    var tarih = new DateTime(yil, ay, gun);
                    var gunlukKayit = puantajKayitlari
                        .FirstOrDefault(p => p.PersonelID == personel.PersonelID && p.Tarih == tarih);

                    satir.GunlukKayitlar[gun] = gunlukKayit;
                }

                viewModel.PersonelSatirlari.Add(satir);
            }

            return viewModel;
        }

        private async Task LoadPuantajDurumlariAsync()
        {
            PuantajDurumlari = await _context.Lookup_PuantajDurumlari
                .OrderBy(p => p.Kod)
                .ToListAsync();
        }

        private async Task LoadDepartmanlarAsync()
        {
            Departmanlar = await _context.Lookup_Departmanlar
                .OrderBy(d => d.Tanim)
                .ToListAsync();
        }

        private async Task LoadGunlukCalismaSaatiAsync()
        {
            var ayar = await _context.Lookup_GenelAyarlar
                .FirstOrDefaultAsync(g => g.AyarKey == "GunlukCalismaSaati");

            if (ayar != null && decimal.TryParse(ayar.AyarValue, 
                System.Globalization.NumberStyles.Any, 
                System.Globalization.CultureInfo.InvariantCulture, 
                out decimal saatDegeri))
            {
                GunlukCalismaSaati = saatDegeri;
            }
            else
            {
                // Varsayılan değer 8 saat
                GunlukCalismaSaati = 8;
            }
        }
    }

    // DTO for form submission
    public class PuantajKayitDto
    {
        public int PersonelID { get; set; }
        public int Ay { get; set; }
        public int Yil { get; set; }
        public int Gun { get; set; }
        public int PuantajDurumID { get; set; }
        public decimal? MesaiSaati { get; set; }
        public int? DepartmanID { get; set; } // Personelin o gün hangi departmanda çalıştığı
    }
}

