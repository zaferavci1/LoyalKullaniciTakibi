using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using LoyalKullaniciTakip.Data;

namespace LoyalKullaniciTakip.Pages.Zimmet
{
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DeleteModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Data.Zimmet Zimmet { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int zimmetId)
        {
            var zimmet = await _context.Zimmetler
                .Include(z => z.Personel)
                .FirstOrDefaultAsync(z => z.ZimmetID == zimmetId);

            if (zimmet == null)
            {
                return NotFound();
            }

            Zimmet = zimmet;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int zimmetId)
        {
            var zimmet = await _context.Zimmetler
                .FirstOrDefaultAsync(z => z.ZimmetID == zimmetId);

            if (zimmet == null)
            {
                return NotFound();
            }

            int personelId = zimmet.PersonelID;

            _context.Zimmetler.Remove(zimmet);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Personel/Details", new { id = personelId });
        }
    }
}

