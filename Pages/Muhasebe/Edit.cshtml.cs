using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LoyalKullaniciTakip.Data;
using System.ComponentModel.DataAnnotations;

namespace LoyalKullaniciTakip.Pages.Muhasebe
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public int PersonelID { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Hareket Tipi zorunludur")]
        public int HareketTipiID { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Tutar zorunludur")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Tutar sıfırdan büyük olmalıdır")]
        public decimal Tutar { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Tarih zorunludur")]
        public DateTime Tarih { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Açıklama zorunludur")]
        public string Aciklama { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(int hareketId)
        {
            var hareket = await _context.MuhasebeHareketleri
                .FirstOrDefaultAsync(h => h.HareketID == hareketId);

            if (hareket == null)
            {
                return NotFound();
            }

            PersonelID = hareket.PersonelID;
            HareketTipiID = hareket.HareketTipiID;
            Tutar = hareket.Tutar;
            Tarih = hareket.Tarih;
            Aciklama = hareket.Aciklama;

            await LoadHareketTipleriAsync();
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

            PersonelID = hareket.PersonelID;

            if (!ModelState.IsValid)
            {
                await LoadHareketTipleriAsync();
                return Page();
            }

            hareket.HareketTipiID = HareketTipiID;
            hareket.Tutar = Tutar;
            hareket.Tarih = Tarih;
            hareket.Aciklama = Aciklama;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await HareketExists(hareketId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("/Personel/Details", new { id = PersonelID });
        }

        private async Task<bool> HareketExists(int hareketId)
        {
            return await _context.MuhasebeHareketleri.AnyAsync(h => h.HareketID == hareketId);
        }

        private async Task LoadHareketTipleriAsync()
        {
            ViewData["HareketTipleriList"] = new SelectList(
                await _context.Lookup_MuhasebeHareketTipi.ToListAsync(),
                "HareketTipiID",
                "Tanim");
        }
    }
}

