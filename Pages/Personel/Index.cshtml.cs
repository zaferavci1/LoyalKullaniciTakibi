using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using LoyalKullaniciTakip.Data;

namespace LoyalKullaniciTakip.Pages.Personel
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Data.Personel> Personeller { get; set; } = default!;

        public async Task OnGetAsync()
        {
            Personeller = await _context.Personeller
                .Include(p => p.Personel_Detay_SGK)
                .ToListAsync();
        }
    }
}
