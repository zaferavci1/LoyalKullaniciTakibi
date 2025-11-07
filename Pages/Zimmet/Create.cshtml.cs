using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using LoyalKullaniciTakip.Data;
using System.ComponentModel.DataAnnotations;

namespace LoyalKullaniciTakip.Pages.Zimmet
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
        [Required(ErrorMessage = "Demirbaş Adı zorunludur")]
        public string DemirbasAdi { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Marka/Model zorunludur")]
        public string MarkaModel { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Seri No zorunludur")]
        public string SeriNo { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Veriliş Tarihi zorunludur")]
        public DateTime VerilisTarihi { get; set; } = DateTime.Now;

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

            // Check if SeriNo already exists
            var existingZimmet = await _context.Zimmetler
                .FirstOrDefaultAsync(z => z.SeriNo == SeriNo);

            if (existingZimmet != null)
            {
                ModelState.AddModelError("SeriNo", "Bu seri numarası zaten kayıtlıdır.");
                return Page();
            }

            var zimmet = new Data.Zimmet
            {
                PersonelID = personelId,
                DemirbasAdi = DemirbasAdi,
                MarkaModel = MarkaModel,
                SeriNo = SeriNo,
                VerilisTarihi = VerilisTarihi,
                IadeTarihi = null
            };

            _context.Zimmetler.Add(zimmet);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Personel/Details", new { id = personelId });
        }
    }
}

