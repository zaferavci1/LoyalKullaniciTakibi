using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LoyalKullaniciTakip.Data;
using System.ComponentModel.DataAnnotations;

namespace LoyalKullaniciTakip.Pages.Egitim
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

        public IActionResult OnGet(int personelId)
        {
            PersonelID = personelId;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int personelId)
        {
            PersonelID = personelId;

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var egitim = new EgitimBilgileri
            {
                PersonelID = personelId,
                Seviye = Seviye,
                OkulAdi = OkulAdi,
                Bolum = Bolum,
                MezuniyetYili = MezuniyetYili
            };

            _context.EgitimBilgileri.Add(egitim);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Personel/Details", new { id = personelId });
        }
    }
}
