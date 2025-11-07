using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using LoyalKullaniciTakip.Data;

namespace LoyalKullaniciTakip.Pages.Muhasebe
{
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DeleteModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public MuhasebeHareketleri Hareket { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int hareketId)
        {
            var hareket = await _context.MuhasebeHareketleri
                .Include(h => h.Personel)
                .Include(h => h.HareketTipi)
                .FirstOrDefaultAsync(h => h.HareketID == hareketId);

            if (hareket == null)
            {
                return NotFound();
            }

            Hareket = hareket;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int hareketId)
        {
            var hareket = await _context.MuhasebeHareketleri
                .FirstOrDefaultAsync(h => h.HareketID == hareketId);

            if (hareket == null)
            {
                return NotFound();
            }

            int personelId = hareket.PersonelID;

            _context.MuhasebeHareketleri.Remove(hareket);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Personel/Details", new { id = personelId });
        }
    }
}

