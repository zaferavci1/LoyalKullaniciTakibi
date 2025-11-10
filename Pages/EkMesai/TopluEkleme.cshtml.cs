using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LoyalKullaniciTakip.Data;
using System.Globalization;

namespace LoyalKullaniciTakip.Pages.EkMesai
{
    public class TopluEklemeModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public TopluEklemeModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public int? SecilenDepartmanID { get; set; }

        [BindProperty]
        public int? SecilenMeslekID { get; set; }

        [BindProperty]
        public List<int> SecilenPersonelIDler { get; set; } = new List<int>();

        [BindProperty]
        public List<DateTime> SecilenTarihler { get; set; } = new List<DateTime>();

        [BindProperty]
        public decimal EkMesaiSaati { get; set; }

        [BindProperty]
        public decimal? ManuelKatsayi { get; set; } // Kullanıcının manuel girdiği katsayı

        [BindProperty]
        public string Aciklama { get; set; } = string.Empty;

        public List<SelectListItem> DepartmanListesi { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> MeslekListesi { get; set; } = new List<SelectListItem>();
        public List<PersonelCheckboxItem> PersonelListesi { get; set; } = new List<PersonelCheckboxItem>();

        // Genel ayarlardan çekilen katsayılar
        public decimal HaftaIciKatsayi { get; set; }
        public decimal CumartesiKatsayi { get; set; }
        public decimal PazarKatsayi { get; set; }

        public async Task OnGetAsync(int? secilenDepartmanID, int? secilenMeslekID)
        {
            // Query string'den gelen parametreleri al
            SecilenDepartmanID = secilenDepartmanID;
            SecilenMeslekID = secilenMeslekID;

            await LoadGenelAyarlarAsync();
            await PrepareDropdownsAsync();
            await LoadPersonelListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await LoadGenelAyarlarAsync();
            await PrepareDropdownsAsync();
            await LoadPersonelListAsync();

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
            if (!SecilenPersonelIDler.Any())
            {
                TempData["ErrorMessage"] = "En az bir personel seçmelisiniz.";
                return Page();
            }

            if (!SecilenTarihler.Any())
            {
                TempData["ErrorMessage"] = "En az bir tarih seçmelisiniz.";
                return Page();
            }

            if (EkMesaiSaati <= 0 || EkMesaiSaati > 24)
            {
                TempData["ErrorMessage"] = "Ek mesai saati 0 ile 24 arasında olmalıdır.";
                return Page();
            }

            // Personel maaş bilgilerini çek
            var personelMaasBilgileri = await _context.Personel_Detay_Muhasebe
                .Where(pm => SecilenPersonelIDler.Contains(pm.PersonelID))
                .ToDictionaryAsync(pm => pm.PersonelID, pm => pm.TemelMaas);

            int basariliKayitSayisi = 0;
            int atlanankayitSayisi = 0;
            var hataMesajlari = new List<string>();

            // Her personel × her tarih kombinasyonu için kayıt oluştur
            foreach (var personelId in SecilenPersonelIDler)
            {
                // Maaş bilgisi kontrolü
                if (!personelMaasBilgileri.TryGetValue(personelId, out var aylikMaas) || aylikMaas <= 0)
                {
                    var personel = await _context.Personeller.FindAsync(personelId);
                    hataMesajlari.Add($"{personel?.Ad} {personel?.Soyad}: Maaş bilgisi bulunamadı veya geçersiz.");
                    continue;
                }

                decimal saatlikUcret = aylikMaas / 225; // Aylık 225 saat varsayımı

                foreach (var tarih in SecilenTarihler)
                {
                    // Duplicate kontrolü
                    var mevcutKayit = await _context.EkMesaiKayitlari
                        .FirstOrDefaultAsync(e => e.PersonelID == personelId && e.Tarih.Date == tarih.Date);

                    if (mevcutKayit != null)
                    {
                        atlanankayitSayisi++;
                        continue;
                    }

                    // Gün tipini ve katsayıyı belirle
                    string gunTipi;
                    decimal katsayi;

                    // Manuel katsayı girilmişse onu kullan
                    if (ManuelKatsayi.HasValue && ManuelKatsayi.Value > 0)
                    {
                        // Gün tipini yine de belirle (raporlama için)
                        if (tarih.DayOfWeek == DayOfWeek.Sunday)
                            gunTipi = "Pazar";
                        else if (tarih.DayOfWeek == DayOfWeek.Saturday)
                            gunTipi = "Cumartesi";
                        else
                            gunTipi = "HaftaIci";

                        katsayi = ManuelKatsayi.Value;
                    }
                    else
                    {
                        // Otomatik katsayı belirleme (Genel Ayarlar'dan)
                        if (tarih.DayOfWeek == DayOfWeek.Sunday)
                        {
                            gunTipi = "Pazar";
                            katsayi = PazarKatsayi;
                        }
                        else if (tarih.DayOfWeek == DayOfWeek.Saturday)
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
                        PersonelID = personelId,
                        Tarih = tarih,
                        EkMesaiSaati = EkMesaiSaati,
                        GunTipi = gunTipi,
                        Katsayi = katsayi,
                        SaatlikUcret = saatlikUcret,
                        HesaplananTutar = hesaplananTutar,
                        Aciklama = Aciklama,
                        OlusturmaTarihi = DateTime.Now
                    };

                    _context.EkMesaiKayitlari.Add(yeniKayit);
                    basariliKayitSayisi++;
                }
            }

            try
            {
                await _context.SaveChangesAsync();

                // Sonuç mesajı
                var mesaj = $"{basariliKayitSayisi} adet ek mesai kaydı başarıyla oluşturuldu.";
                if (atlanankayitSayisi > 0)
                {
                    mesaj += $" {atlanankayitSayisi} adet kayıt zaten mevcut olduğu için atlandı.";
                }
                if (hataMesajlari.Any())
                {
                    mesaj += "<br/><strong>Hatalar:</strong><br/>" + string.Join("<br/>", hataMesajlari);
                }

                TempData["SuccessMessage"] = mesaj;

                return RedirectToPage("/EkMesai/Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Veritabanı hatası: {ex.Message}. İç hata: {ex.InnerException?.Message}";
                return Page();
            }
        }

        public async Task<IActionResult> OnGetPersonellerByFiltersAsync(int? departmanId, int? meslekId)
        {
            var query = _context.Personeller.AsQueryable();

            if (departmanId.HasValue && departmanId.Value > 0)
            {
                query = query.Where(p => p.DepartmanID == departmanId.Value);
            }

            if (meslekId.HasValue && meslekId.Value > 0)
            {
                query = query.Where(p => p.MeslekID == meslekId.Value);
            }

            var personeller = await query
                .OrderBy(p => p.Ad)
                .ThenBy(p => p.Soyad)
                .Select(p => new
                {
                    p.PersonelID,
                    AdSoyad = p.Ad + " " + p.Soyad
                })
                .ToListAsync();

            return new JsonResult(personeller);
        }

        private async Task PrepareDropdownsAsync()
        {
            // Departman listesi
            var departmanlar = await _context.Lookup_Departmanlar
                .OrderBy(d => d.Tanim)
                .Select(d => new SelectListItem
                {
                    Value = d.DepartmanID.ToString(),
                    Text = d.Tanim,
                    Selected = SecilenDepartmanID.HasValue && SecilenDepartmanID.Value == d.DepartmanID
                })
                .ToListAsync();

            DepartmanListesi = new List<SelectListItem>
            {
                new SelectListItem { Value = "0", Text = "Tüm Departmanlar", Selected = !SecilenDepartmanID.HasValue || SecilenDepartmanID.Value == 0 }
            };
            DepartmanListesi.AddRange(departmanlar);

            // Meslek listesi
            var meslekler = await _context.Lookup_Meslekler
                .OrderBy(m => m.Tanim)
                .Select(m => new SelectListItem
                {
                    Value = m.MeslekID.ToString(),
                    Text = m.Tanim,
                    Selected = SecilenMeslekID.HasValue && SecilenMeslekID.Value == m.MeslekID
                })
                .ToListAsync();

            MeslekListesi = new List<SelectListItem>
            {
                new SelectListItem { Value = "0", Text = "Tüm Meslekler", Selected = !SecilenMeslekID.HasValue || SecilenMeslekID.Value == 0 }
            };
            MeslekListesi.AddRange(meslekler);
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

        private async Task LoadPersonelListAsync()
        {
            var query = _context.Personeller
                .Include(p => p.Departman)
                .Include(p => p.Meslek)
                .AsQueryable();

            // Filtre uygula
            if (SecilenDepartmanID.HasValue && SecilenDepartmanID.Value > 0)
            {
                query = query.Where(p => p.DepartmanID == SecilenDepartmanID.Value);
            }

            if (SecilenMeslekID.HasValue && SecilenMeslekID.Value > 0)
            {
                query = query.Where(p => p.MeslekID == SecilenMeslekID.Value);
            }

            PersonelListesi = await query
                .OrderBy(p => p.Ad)
                .ThenBy(p => p.Soyad)
                .Select(p => new PersonelCheckboxItem
                {
                    PersonelID = p.PersonelID,
                    AdSoyad = $"{p.Ad} {p.Soyad}",
                    Departman = p.Departman != null ? p.Departman.Tanim : "-",
                    Meslek = p.Meslek != null ? p.Meslek.Tanim : "-",
                    IsSelected = SecilenPersonelIDler.Contains(p.PersonelID)
                })
                .ToListAsync();
        }
    }

    public class PersonelCheckboxItem
    {
        public int PersonelID { get; set; }
        public string AdSoyad { get; set; } = string.Empty;
        public string Departman { get; set; } = string.Empty;
        public string Meslek { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
    }
}
