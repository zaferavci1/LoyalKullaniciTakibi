using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using LoyalKullaniciTakip.Data;

namespace LoyalKullaniciTakip.Pages.Egitim
{
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DeleteModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public EgitimBilgileri Egitim { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int egitimId)
        {
            var egitim = await _context.EgitimBilgileri
                .Include(e => e.Personel)
                .FirstOrDefaultAsync(e => e.EgitimKayitID == egitimId);

            if (egitim == null)
            {
                return NotFound();
            }

            Egitim = egitim;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int egitimId)
        {
            var egitim = await _context.EgitimBilgileri
                .FirstOrDefaultAsync(e => e.EgitimKayitID == egitimId);

            if (egitim == null)
            {
                return NotFound();
            }

            int personelId = egitim.PersonelID;

            _context.EgitimBilgileri.Remove(egitim);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Personel/Details", new { id = personelId });
        }
    }
}
