using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using LoyalKullaniciTakip.Data;
using System.ComponentModel.DataAnnotations;

namespace LoyalKullaniciTakip.Pages.Zimmet
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
        public DateTime VerilisTarihi { get; set; }

        [BindProperty]
        public DateTime? IadeTarihi { get; set; }

        public async Task<IActionResult> OnGetAsync(int zimmetId)
        {
            var zimmet = await _context.Zimmetler
                .FirstOrDefaultAsync(z => z.ZimmetID == zimmetId);

            if (zimmet == null)
            {
                return NotFound();
            }

            PersonelID = zimmet.PersonelID;
            DemirbasAdi = zimmet.DemirbasAdi;
            MarkaModel = zimmet.MarkaModel;
            SeriNo = zimmet.SeriNo;
            VerilisTarihi = zimmet.VerilisTarihi;
            IadeTarihi = zimmet.IadeTarihi;

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

            PersonelID = zimmet.PersonelID;

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Check if SeriNo already exists (excluding current record)
            var existingZimmet = await _context.Zimmetler
                .FirstOrDefaultAsync(z => z.SeriNo == SeriNo && z.ZimmetID != zimmetId);

            if (existingZimmet != null)
            {
                ModelState.AddModelError("SeriNo", "Bu seri numarası zaten kayıtlıdır.");
                return Page();
            }

            zimmet.DemirbasAdi = DemirbasAdi;
            zimmet.MarkaModel = MarkaModel;
            zimmet.SeriNo = SeriNo;
            zimmet.VerilisTarihi = VerilisTarihi;
            zimmet.IadeTarihi = IadeTarihi;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ZimmetExists(zimmetId))
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

        private async Task<bool> ZimmetExists(int zimmetId)
        {
            return await _context.Zimmetler.AnyAsync(z => z.ZimmetID == zimmetId);
        }
    }
}

