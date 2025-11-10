using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LoyalKullaniciTakip.Data;
using System.Globalization;

namespace LoyalKullaniciTakip.Pages.EkMesai
{
    public class BireyselEklemeModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public BireyselEklemeModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public int SecilenPersonelID { get; set; }

        [BindProperty]
        public DateTime SecilenTarih { get; set; }

        [BindProperty]
        public decimal EkMesaiSaati { get; set; }

        [BindProperty]
        public string Aciklama { get; set; } = string.Empty;

        [BindProperty]
        public decimal? ManuelKatsayi { get; set; } // Kullanıcının manuel girdiği katsayı

        public List<SelectListItem> PersonelListesi { get; set; } = new List<SelectListItem>();
        public string TahminiGunTipi { get; set; } = string.Empty;
        public decimal TahminiKatsayi { get; set; }
        public decimal TahminiSaatlikUcret { get; set; }
        public decimal TahminiTutar { get; set; }

        // Genel ayarlardan çekilen katsayılar
        public decimal HaftaIciKatsayi { get; set; }
        public decimal CumartesiKatsayi { get; set; }
        public decimal PazarKatsayi { get; set; }

        public async Task OnGetAsync()
        {
            SecilenTarih = DateTime.Today;
            await LoadGenelAyarlarAsync();
            await PrepareDropdownsAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await LoadGenelAyarlarAsync();
            await PrepareDropdownsAsync();

            if (!ModelState.IsValid)
            {
                // ModelState hatalarını göster
                var errors = string.Join(", ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                TempData["ErrorMessage"] = $"Form doğrulama hatası: {errors}";
                return Page();
            }

            // Validasyon
            if (SecilenPersonelID <= 0)
            {
                TempData["ErrorMessage"] = "Lütfen bir personel seçin.";
                return Page();
            }

            if (EkMesaiSaati <= 0 || EkMesaiSaati > 24)
            {
                TempData["ErrorMessage"] = "Ek mesai saati 0 ile 24 arasında olmalıdır.";
                return Page();
            }

            // Duplicate kontrolü
            var mevcutKayit = await _context.EkMesaiKayitlari
                .FirstOrDefaultAsync(e => e.PersonelID == SecilenPersonelID && e.Tarih.Date == SecilenTarih.Date);

            if (mevcutKayit != null)
            {
                TempData["ErrorMessage"] = $"Bu personel için {SecilenTarih:dd.MM.yyyy} tarihinde zaten ek mesai kaydı mevcut.";
                return Page();
            }

            // Personel maaş bilgisini çek
            var personelMuhasebe = await _context.Personel_Detay_Muhasebe
                .FirstOrDefaultAsync(pm => pm.PersonelID == SecilenPersonelID);

            if (personelMuhasebe == null || personelMuhasebe.TemelMaas <= 0)
            {
                TempData["ErrorMessage"] = "Seçilen personelin maaş bilgisi bulunamadı veya geçersiz.";
                return Page();
            }

            decimal saatlikUcret = personelMuhasebe.TemelMaas / 225; // Aylık 225 saat varsayımı

            // Gün tipini ve katsayıyı belirle
            string gunTipi;
            decimal katsayi;

            // Manuel katsayı girilmişse onu kullan
            if (ManuelKatsayi.HasValue && ManuelKatsayi.Value > 0)
            {
                // Gün tipini yine de belirle (raporlama için)
                if (SecilenTarih.DayOfWeek == DayOfWeek.Sunday)
                    gunTipi = "Pazar";
                else if (SecilenTarih.DayOfWeek == DayOfWeek.Saturday)
                    gunTipi = "Cumartesi";
                else
                    gunTipi = "HaftaIci";

                katsayi = ManuelKatsayi.Value;
            }
            else
            {
                // Otomatik katsayı belirleme (Genel Ayarlar'dan)
                if (SecilenTarih.DayOfWeek == DayOfWeek.Sunday)
                {
                    gunTipi = "Pazar";
                    katsayi = PazarKatsayi;
                }
                else if (SecilenTarih.DayOfWeek == DayOfWeek.Saturday)
                {
                    gunTipi = "Cumartesi";
                    katsayi = CumartesiKatsayi;
                }
                else
                {
                    gunTipi = "HaftaIci";
                    katsayi = HaftaIciKatsayi;
                }
            }

            // Tutarı hesapla
            decimal hesaplananTutar = saatlikUcret * EkMesaiSaati * katsayi;

            // Yeni kayıt oluştur
            var yeniKayit = new EkMesaiKayitlari
            {
                PersonelID = SecilenPersonelID,
                Tarih = SecilenTarih,
                EkMesaiSaati = EkMesaiSaati,
                GunTipi = gunTipi,
                Katsayi = katsayi,
                SaatlikUcret = saatlikUcret,
                HesaplananTutar = hesaplananTutar,
                Aciklama = Aciklama,
                OlusturmaTarihi = DateTime.Now
            };

            try
            {
                _context.EkMesaiKayitlari.Add(yeniKayit);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Ek mesai kaydı başarıyla oluşturuldu. Tutar: {hesaplananTutar:N2} ₺";

                return RedirectToPage("/EkMesai/Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Veritabanı hatası: {ex.Message}. İç hata: {ex.InnerException?.Message}";
                return Page();
            }
        }

        public async Task<IActionResult> OnGetPreviewAsync(int personelId, string tarih, decimal saat)
        {
            if (personelId <= 0 || string.IsNullOrEmpty(tarih) || saat <= 0)
            {
                return new JsonResult(new { success = false, message = "Geçersiz parametreler" });
            }

            if (!DateTime.TryParseExact(tarih, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var tarihDegeri))
            {
                return new JsonResult(new { success = false, message = "Geçersiz tarih formatı" });
            }

            // Personel maaş bilgisini çek
            var personelMuhasebe = await _context.Personel_Detay_Muhasebe
                .FirstOrDefaultAsync(pm => pm.PersonelID == personelId);

            if (personelMuhasebe == null || personelMuhasebe.TemelMaas <= 0)
            {
                return new JsonResult(new { success = false, message = "Maaş bilgisi bulunamadı" });
            }

            decimal saatlikUcret = personelMuhasebe.TemelMaas / 225;

            // Genel ayarları yükle
            await LoadGenelAyarlarAsync();

            // Gün tipini ve katsayıyı belirle
            string gunTipi;
            string gunTipiAciklama;
            decimal katsayi;

            if (tarihDegeri.DayOfWeek == DayOfWeek.Sunday)
            {
                gunTipi = "Pazar";
                gunTipiAciklama = "Pazar/Tatil";
                katsayi = PazarKatsayi;
            }
            else if (tarihDegeri.DayOfWeek == DayOfWeek.Saturday)
            {
                gunTipi = "Cumartesi";
                gunTipiAciklama = "Cumartesi";
                katsayi = CumartesiKatsayi;
            }
            else
            {
                gunTipi = "HaftaIci";
                gunTipiAciklama = "Hafta İçi";
                katsayi = HaftaIciKatsayi;
            }

            // Tutarı hesapla
            decimal hesaplananTutar = saatlikUcret * saat * katsayi;

            return new JsonResult(new
            {
                success = true,
                gunTipi = gunTipiAciklama,
                katsayi = katsayi.ToString("F2"),
                saatlikUcret = saatlikUcret.ToString("N2"),
                tutar = hesaplananTutar.ToString("N2")
            });
        }

        private async Task LoadGenelAyarlarAsync()
        {
            var genelAyarlar = await _context.Lookup_GenelAyarlar.ToListAsync();

            HaftaIciKatsayi = decimal.Parse(
                genelAyarlar.FirstOrDefault(a => a.AyarKey == "HaftaIciMesaiCarpani")?.AyarValue ?? "1.5",
                CultureInfo.InvariantCulture
            );

            CumartesiKatsayi = decimal.Parse(
                genelAyarlar.FirstOrDefault(a => a.AyarKey == "CumartesiMesaiCarpani")?.AyarValue ?? "1.75",
                CultureInfo.InvariantCulture
            );

            PazarKatsayi = decimal.Parse(
                genelAyarlar.FirstOrDefault(a => a.AyarKey == "PazarMesaiCarpani")?.AyarValue ?? "2.0",
                CultureInfo.InvariantCulture
            );
        }

        private async Task PrepareDropdownsAsync()
        {
            // Personel listesi
            var personeller = await _context.Personeller
                .OrderBy(p => p.Ad)
                .ThenBy(p => p.Soyad)
                .Select(p => new SelectListItem
                {
                    Value = p.PersonelID.ToString(),
                    Text = $"{p.Ad} {p.Soyad}",
                    Selected = p.PersonelID == SecilenPersonelID
                })
                .ToListAsync();

            PersonelListesi = new List<SelectListItem>
            {
                new SelectListItem { Value = "0", Text = "-- Personel Seçin --", Selected = SecilenPersonelID == 0 }
            };
            PersonelListesi.AddRange(personeller);
        }
    }
}
