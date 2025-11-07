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
        public int? EditingIzinKuralId { get; set; }

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

        private async Task LoadDataAsync()
        {
            GenelAyarlar = await _context.Lookup_GenelAyarlar
                .OrderBy(a => a.AyarKey)
                .ToListAsync();

            IzinKurallari = await _context.Lookup_KidemIzinHakedis
                .OrderBy(k => k.MinKidemYili)
                .ToListAsync();
        }
    }
}
