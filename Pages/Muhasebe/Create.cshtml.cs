using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LoyalKullaniciTakip.Data;
using System.ComponentModel.DataAnnotations;

namespace LoyalKullaniciTakip.Pages.Muhasebe
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
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
        public DateTime Tarih { get; set; } = DateTime.Now;

        [BindProperty]
        [Required(ErrorMessage = "Açıklama zorunludur")]
        public string Aciklama { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(int personelId)
        {
            PersonelID = personelId;
            await LoadHareketTipleriAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int personelId)
        {
            PersonelID = personelId;

            if (!ModelState.IsValid)
            {
                await LoadHareketTipleriAsync();
                return Page();
            }

            var hareket = new MuhasebeHareketleri
            {
                PersonelID = personelId,
                HareketTipiID = HareketTipiID,
                Tutar = Tutar,
                Tarih = Tarih,
                Aciklama = Aciklama
            };

            _context.MuhasebeHareketleri.Add(hareket);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Personel/Details", new { id = personelId });
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

