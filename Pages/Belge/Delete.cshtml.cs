using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using LoyalKullaniciTakip.Data;

namespace LoyalKullaniciTakip.Pages.Belge
{
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public DeleteModel(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [BindProperty]
        public Belgeler Belge { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int belgeId)
        {
            var belge = await _context.Belgeler
                .Include(b => b.Personel)
                .Include(b => b.BelgeKategori)
                .FirstOrDefaultAsync(b => b.BelgeID == belgeId);

            if (belge == null)
            {
                return NotFound();
            }

            Belge = belge;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int belgeId)
        {
            var belge = await _context.Belgeler
                .FirstOrDefaultAsync(b => b.BelgeID == belgeId);

            if (belge == null)
            {
                return NotFound();
            }

            int personelId = belge.PersonelID;

            try
            {
                // Fiziksel dosyayı sil
                var dosyaPath = Path.Combine(_environment.WebRootPath, belge.DosyaYolu.TrimStart('/'));
                if (System.IO.File.Exists(dosyaPath))
                {
                    System.IO.File.Delete(dosyaPath);
                }

                // Veritabanından sil
                _context.Belgeler.Remove(belge);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Hata durumunda log tutulabilir
                ModelState.AddModelError(string.Empty, $"Belge silinirken hata oluştu: {ex.Message}");
                return Page();
            }

            return RedirectToPage("/Personel/Details", new { id = personelId });
        }
    }
}

