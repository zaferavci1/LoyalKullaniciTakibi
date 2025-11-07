using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using LoyalKullaniciTakip.Data;

namespace LoyalKullaniciTakip.Pages.Izin
{
    public class ListModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ListModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<IzinTalepleri> IzinTalepleri { get; set; } = default!;

        public async Task OnGetAsync()
        {
            IzinTalepleri = await _context.IzinTalepleri
                .Include(i => i.Personel)
                .Include(i => i.IzinTipi)
                .Include(i => i.OnaylayanPersonel)
                .OrderByDescending(i => i.TalepTarihi)
                .ToListAsync();
        }
    }
}

