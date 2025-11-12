using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using LoyalKullaniciTakip.Data;
using LoyalKullaniciTakip.Data.Lookups;
using System.ComponentModel.DataAnnotations;

namespace LoyalKullaniciTakip.Pages.Ayarlar
{
    // [Authorize(Roles = "Admin")] // Test sonrası aktif edilecek
    public class GenelAyarlarModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public GenelAyarlarModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Lookup_GenelAyarlar> GenelAyarlar { get; set; } = new List<Lookup_GenelAyarlar>();
        public List<Lookup_KidemIzinHakedis> IzinKurallari { get; set; } = new List<Lookup_KidemIzinHakedis>();
        public List<Lookup_Departmanlar> Departmanlar { get; set; } = new List<Lookup_Departmanlar>();
        public List<Lookup_Meslekler> Meslekler { get; set; } = new List<Lookup_Meslekler>();
        public List<Lookup_MeslekKodlari> SGKMeslekKodlari { get; set; } = new List<Lookup_MeslekKodlari>();
        public int? EditingIzinKuralId { get; set; }
        public int? EditingDepartmanId { get; set; }
        public int? EditingMeslekId { get; set; }
        public int? EditingSGKKodId { get; set; }

        [BindProperty]
        public Dictionary<string, string> AyarGuncelleme { get; set; } = new Dictionary<string, string>();

        public async Task OnGetAsync()
        {
            await LoadDataAsync();
        }

        public async Task<IActionResult> OnGetEditIzinKuralAsync(int id)
        {
            EditingIzinKuralId = id;
            await LoadDataAsync();
            return Page();
        }

        public IActionResult OnGetCancelEditIzinKural()
        {
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (AyarGuncelleme == null || !AyarGuncelleme.Any())
            {
                TempData["ErrorMessage"] = "Güncellenecek ayar bulunamadı.";
                return RedirectToPage();
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    foreach (var kvp in AyarGuncelleme)
                    {
                        var ayarKey = kvp.Key;
                        var ayarValue = kvp.Value;

                        // Veritabanında ilgili ayarı bul
                        var mevcutAyar = await _context.Lookup_GenelAyarlar
                            .FirstOrDefaultAsync(a => a.AyarKey == ayarKey);

                        if (mevcutAyar != null)
                        {
                            // AyarValue'yu güncelle
                            mevcutAyar.AyarValue = ayarValue;
                        }
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    TempData["SuccessMessage"] = "Genel ayarlar başarıyla güncellendi.";
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    TempData["ErrorMessage"] = $"Güncelleme sırasında hata oluştu: {ex.Message}";
                }
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateIzinKuralAsync(
            [Required] int KidemIzinID,
            [Required, Range(1, 99)] int MinKidemYili,
            [Required, Range(1, 99)] int MaxKidemYili,
            [Required, Range(1, 365)] int HakedilenGunSayisi)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Lütfen tüm alanları doğru şekilde doldurun.";
                EditingIzinKuralId = KidemIzinID;
                await LoadDataAsync();
                return Page();
            }

            if (MinKidemYili >= MaxKidemYili)
            {
                TempData["ErrorMessage"] = "Minimum kıdem yılı, maximum kıdem yılından küçük olmalıdır.";
                EditingIzinKuralId = KidemIzinID;
                await LoadDataAsync();
                return Page();
            }

            var kural = await _context.Lookup_KidemIzinHakedis.FindAsync(KidemIzinID);
            if (kural == null)
            {
                TempData["ErrorMessage"] = "Kural bulunamadı.";
                return RedirectToPage();
            }

            kural.MinKidemYili = MinKidemYili;
            kural.MaxKidemYili = MaxKidemYili;
            kural.HakedilenGunSayisi = HakedilenGunSayisi;

            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "İzin kuralı başarıyla güncellendi.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Güncelleme sırasında hata oluştu: {ex.Message}";
            }

            return RedirectToPage();
        }

        // Departman Management
        public async Task<IActionResult> OnPostAddDepartmanAsync(string Tanim)
        {
            if (string.IsNullOrWhiteSpace(Tanim))
            {
                TempData["ErrorMessage"] = "Departman adı boş olamaz.";
                await LoadDataAsync();
                return Page();
            }

            var existingDepartman = await _context.Lookup_Departmanlar
                .FirstOrDefaultAsync(d => d.Tanim == Tanim);

            if (existingDepartman != null)
            {
                TempData["ErrorMessage"] = "Bu departman zaten mevcut.";
                await LoadDataAsync();
                return Page();
            }

            var yeniDepartman = new Lookup_Departmanlar { Tanim = Tanim };
            _context.Lookup_Departmanlar.Add(yeniDepartman);

            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Departman başarıyla eklendi.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Departman eklenirken hata oluştu: {ex.Message}";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateDepartmanAsync(int id, string Tanim)
        {
            if (string.IsNullOrWhiteSpace(Tanim))
            {
                TempData["ErrorMessage"] = "Departman adı boş olamaz.";
                EditingDepartmanId = id;
                await LoadDataAsync();
                return Page();
            }

            var departman = await _context.Lookup_Departmanlar.FindAsync(id);
            if (departman == null)
            {
                TempData["ErrorMessage"] = "Departman bulunamadı.";
                return RedirectToPage();
            }

            departman.Tanim = Tanim;

            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Departman başarıyla güncellendi.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Güncelleme sırasında hata oluştu: {ex.Message}";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteDepartmanAsync(int id)
        {
            var departman = await _context.Lookup_Departmanlar.FindAsync(id);
            if (departman == null)
            {
                TempData["ErrorMessage"] = "Departman bulunamadı.";
                return RedirectToPage();
            }

            // Check if department is in use
            var inUse = await _context.Personeller.AnyAsync(p => p.DepartmanID == id);
            if (inUse)
            {
                TempData["ErrorMessage"] = "Bu departman kullanımda olduğu için silinemez.";
                return RedirectToPage();
            }

            _context.Lookup_Departmanlar.Remove(departman);

            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Departman başarıyla silindi.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Silme sırasında hata oluştu: {ex.Message}";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnGetEditDepartmanAsync(int id)
        {
            EditingDepartmanId = id;
            await LoadDataAsync();
            return Page();
        }

        public IActionResult OnGetCancelEditDepartman()
        {
            return RedirectToPage();
        }

        // Meslek Management
        public async Task<IActionResult> OnPostAddMeslekAsync(string Tanim)
        {
            if (string.IsNullOrWhiteSpace(Tanim))
            {
                TempData["ErrorMessage"] = "Meslek adı boş olamaz.";
                return RedirectToPage();
            }

            var existingMeslek = await _context.Lookup_Meslekler
                .FirstOrDefaultAsync(m => m.Tanim == Tanim);

            if (existingMeslek != null)
            {
                TempData["ErrorMessage"] = "Bu meslek zaten mevcut.";
                return RedirectToPage();
            }

            var yeniMeslek = new Lookup_Meslekler { Tanim = Tanim };
            _context.Lookup_Meslekler.Add(yeniMeslek);

            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Meslek başarıyla eklendi.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Meslek eklenirken hata oluştu: {ex.Message}";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateMeslekAsync(int id, string Tanim)
        {
            if (string.IsNullOrWhiteSpace(Tanim))
            {
                TempData["ErrorMessage"] = "Meslek adı boş olamaz.";
                EditingMeslekId = id;
                await LoadDataAsync();
                return Page();
            }

            var meslek = await _context.Lookup_Meslekler.FindAsync(id);
            if (meslek == null)
            {
                TempData["ErrorMessage"] = "Meslek bulunamadı.";
                return RedirectToPage();
            }

            meslek.Tanim = Tanim;

            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Meslek başarıyla güncellendi.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Güncelleme sırasında hata oluştu: {ex.Message}";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteMeslekAsync(int id)
        {
            var meslek = await _context.Lookup_Meslekler.FindAsync(id);
            if (meslek == null)
            {
                TempData["ErrorMessage"] = "Meslek bulunamadı.";
                return RedirectToPage();
            }

            // Check if profession is in use
            var inUse = await _context.Personeller.AnyAsync(p => p.MeslekID == id);
            if (inUse)
            {
                TempData["ErrorMessage"] = "Bu meslek kullanımda olduğu için silinemez.";
                return RedirectToPage();
            }

            _context.Lookup_Meslekler.Remove(meslek);

            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Meslek başarıyla silindi.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Silme sırasında hata oluştu: {ex.Message}";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnGetEditMeslekAsync(int id)
        {
            EditingMeslekId = id;
            await LoadDataAsync();
            return Page();
        }

        public IActionResult OnGetCancelEditMeslek()
        {
            return RedirectToPage();
        }

        private async Task LoadDataAsync()
        {
            GenelAyarlar = await _context.Lookup_GenelAyarlar
                .OrderBy(a => a.AyarKey)
                .ToListAsync();

            IzinKurallari = await _context.Lookup_KidemIzinHakedis
                .OrderBy(k => k.MinKidemYili)
                .ToListAsync();

            Departmanlar = await _context.Lookup_Departmanlar
                .OrderBy(d => d.Tanim)
                .ToListAsync();

            Meslekler = await _context.Lookup_Meslekler
                .OrderBy(m => m.Tanim)
                .ToListAsync();

            SGKMeslekKodlari = await _context.Lookup_MeslekKodlari
                .OrderBy(s => s.Kod)
                .ToListAsync();
        }

        #region SGK Meslek Kodları CRUD İşlemleri

        public async Task<IActionResult> OnGetEditSGKKoduAsync(int id)
        {
            EditingSGKKodId = id;
            await LoadDataAsync();
            return Page();
        }

        public IActionResult OnGetCancelEditSGKKodu()
        {
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAddSGKKoduAsync(string Kod, string Tanim)
        {
            if (string.IsNullOrWhiteSpace(Kod) || string.IsNullOrWhiteSpace(Tanim))
            {
                TempData["ErrorMessage"] = "SGK Kodu ve Tanım alanları zorunludur.";
                return RedirectToPage();
            }

            // Kod unique olmalı
            var mevcutKod = await _context.Lookup_MeslekKodlari
                .FirstOrDefaultAsync(s => s.Kod == Kod.Trim());

            if (mevcutKod != null)
            {
                TempData["ErrorMessage"] = $"'{Kod}' SGK kodu zaten mevcut.";
                return RedirectToPage();
            }

            var yeniSGKKod = new Lookup_MeslekKodlari
            {
                Kod = Kod.Trim(),
                Tanim = Tanim.Trim()
            };

            _context.Lookup_MeslekKodlari.Add(yeniSGKKod);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "SGK Meslek Kodu başarıyla eklendi.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateSGKKoduAsync(int id, string Kod, string Tanim)
        {
            if (string.IsNullOrWhiteSpace(Kod) || string.IsNullOrWhiteSpace(Tanim))
            {
                TempData["ErrorMessage"] = "SGK Kodu ve Tanım alanları zorunludur.";
                return RedirectToPage();
            }

            var sgkKod = await _context.Lookup_MeslekKodlari.FindAsync(id);
            if (sgkKod == null)
            {
                TempData["ErrorMessage"] = "SGK Meslek Kodu bulunamadı.";
                return RedirectToPage();
            }

            // Eğer kod değişiyorsa, yeni kodun unique olduğundan emin ol
            if (sgkKod.Kod != Kod.Trim())
            {
                var mevcutKod = await _context.Lookup_MeslekKodlari
                    .FirstOrDefaultAsync(s => s.Kod == Kod.Trim() && s.MeslekKoduID != id);

                if (mevcutKod != null)
                {
                    TempData["ErrorMessage"] = $"'{Kod}' SGK kodu zaten başka bir kayıtta mevcut.";
                    EditingSGKKodId = id;
                    await LoadDataAsync();
                    return Page();
                }
            }

            sgkKod.Kod = Kod.Trim();
            sgkKod.Tanim = Tanim.Trim();

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "SGK Meslek Kodu başarıyla güncellendi.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteSGKKoduAsync(int id)
        {
            var sgkKod = await _context.Lookup_MeslekKodlari.FindAsync(id);
            if (sgkKod == null)
            {
                TempData["ErrorMessage"] = "SGK Meslek Kodu bulunamadı.";
                return RedirectToPage();
            }

            // Personel kullanıyor mu kontrol et
            var personelKullaniyor = await _context.Personel_Detay_SGK
                .AnyAsync(p => p.SGKMeslekKoduID == id);

            if (personelKullaniyor)
            {
                TempData["ErrorMessage"] = "Bu SGK Meslek Kodu personellerde kullanıldığı için silinemez.";
                return RedirectToPage();
            }

            _context.Lookup_MeslekKodlari.Remove(sgkKod);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "SGK Meslek Kodu başarıyla silindi.";
            return RedirectToPage();
        }

        #endregion
    }
}
