using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LoyalKullaniciTakip.Data;
using System.ComponentModel.DataAnnotations;

namespace LoyalKullaniciTakip.Pages.Belge
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public EditModel(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public int PersonelID { get; set; }
        public string MevcutDosyaYolu { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Belge adı zorunludur")]
        public string BelgeAdi { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Belge kategorisi zorunludur")]
        public int BelgeKategoriID { get; set; }

        [BindProperty]
        public IFormFile? YeniDosya { get; set; }

        public async Task<IActionResult> OnGetAsync(int belgeId)
        {
            var belge = await _context.Belgeler
                .FirstOrDefaultAsync(b => b.BelgeID == belgeId);

            if (belge == null)
            {
                return NotFound();
            }

            PersonelID = belge.PersonelID;
            BelgeAdi = belge.BelgeAdi;
            BelgeKategoriID = belge.BelgeKategoriID;
            MevcutDosyaYolu = belge.DosyaYolu;

            await LoadBelgeKategorileriAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int belgeId)
        {
            var belge = await _context.Belgeler
                .FirstOrDefaultAsync(b => b.BelgeID == belgeId);

            if (belge == null)
            {
                return NotFound();
            }

            PersonelID = belge.PersonelID;
            MevcutDosyaYolu = belge.DosyaYolu;

            if (!ModelState.IsValid)
            {
                await LoadBelgeKategorileriAsync();
                return Page();
            }

            belge.BelgeAdi = BelgeAdi;
            belge.BelgeKategoriID = BelgeKategoriID;

            // Yeni dosya yüklendiyse
            if (YeniDosya != null && YeniDosya.Length > 0)
            {
                try
                {
                    // Eski dosyayı sil
                    var eskiDosyaPath = Path.Combine(_environment.WebRootPath, belge.DosyaYolu.TrimStart('/'));
                    if (System.IO.File.Exists(eskiDosyaPath))
                    {
                        System.IO.File.Delete(eskiDosyaPath);
                    }

                    // Personel klasörünü oluştur
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "ozluk", PersonelID.ToString());
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Yeni dosyayı kaydet
                    var uniqueFileName = $"{Guid.NewGuid()}_{YeniDosya.FileName}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await YeniDosya.CopyToAsync(fileStream);
                    }

                    belge.DosyaYolu = $"/uploads/ozluk/{PersonelID}/{uniqueFileName}";
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"Dosya yüklenirken hata oluştu: {ex.Message}");
                    await LoadBelgeKategorileriAsync();
                    return Page();
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await BelgeExists(belgeId))
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

        private async Task<bool> BelgeExists(int belgeId)
        {
            return await _context.Belgeler.AnyAsync(b => b.BelgeID == belgeId);
        }

        private async Task LoadBelgeKategorileriAsync()
        {
            ViewData["BelgeKategorileriList"] = new SelectList(
                await _context.Lookup_BelgeKategorileri.ToListAsync(),
                "BelgeKategoriID",
                "Tanim");
        }
    }
}

