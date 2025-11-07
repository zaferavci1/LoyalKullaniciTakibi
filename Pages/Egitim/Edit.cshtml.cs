using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using LoyalKullaniciTakip.Data;
using System.ComponentModel.DataAnnotations;

namespace LoyalKullaniciTakip.Pages.Egitim
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
        [Required(ErrorMessage = "Seviye alanı zorunludur")]
        public string Seviye { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Okul Adı zorunludur")]
        public string OkulAdi { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Bölüm zorunludur")]
        public string Bolum { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Mezuniyet Yılı zorunludur")]
        [Range(1950, 2100, ErrorMessage = "Geçerli bir yıl giriniz")]
        public int MezuniyetYili { get; set; }

        public async Task<IActionResult> OnGetAsync(int egitimId)
        {
            var egitim = await _context.EgitimBilgileri
                .FirstOrDefaultAsync(e => e.EgitimKayitID == egitimId);

            if (egitim == null)
            {
                return NotFound();
            }

            PersonelID = egitim.PersonelID;
            Seviye = egitim.Seviye;
            OkulAdi = egitim.OkulAdi;
            Bolum = egitim.Bolum;
            MezuniyetYili = egitim.MezuniyetYili;

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

            PersonelID = egitim.PersonelID;

            if (!ModelState.IsValid)
            {
                return Page();
            }

            egitim.Seviye = Seviye;
            egitim.OkulAdi = OkulAdi;
            egitim.Bolum = Bolum;
            egitim.MezuniyetYili = MezuniyetYili;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await EgitimExists(egitimId))
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

        private async Task<bool> EgitimExists(int egitimId)
        {
            return await _context.EgitimBilgileri.AnyAsync(e => e.EgitimKayitID == egitimId);
        }
    }
}
