using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LoyalKullaniciTakip.Data;

namespace LoyalKullaniciTakip.Pages.Personel
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public DetailsModel(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public Data.Personel Personel { get; set; } = default!;

        [BindProperty]
        public IFormFile? UploadedFile { get; set; }

        [BindProperty]
        public string BelgeAdi { get; set; } = string.Empty;

        [BindProperty]
        public int BelgeKategoriID { get; set; }

        [BindProperty]
        public IFormFile? ProfilFotografi { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var personel = await _context.Personeller
                .Include(p => p.Personel_Detay_SGK)
                .Include(p => p.Personel_Detay_Muhasebe)
                .Include(p => p.Departman)
                .Include(p => p.Meslek)
                .Include(p => p.IletisimBilgileri)
                    .ThenInclude(i => i.IletisimTipi)
                .Include(p => p.Belgeler)
                    .ThenInclude(b => b.BelgeKategori)
                .Include(p => p.EgitimBilgileri)
                .Include(p => p.Zimmetler)
                .Include(p => p.MuhasebeHareketleri)
                    .ThenInclude(m => m.HareketTipi)
                .FirstOrDefaultAsync(p => p.PersonelID == id);

            if (personel == null)
            {
                return NotFound();
            }

            Personel = personel;
            await LoadBelgeKategorileriAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostUploadBelgeAsync(int id)
        {
            if (UploadedFile == null || UploadedFile.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "Lütfen bir dosya seçiniz.");
                await OnGetAsync(id);
                return Page();
            }

            if (string.IsNullOrWhiteSpace(BelgeAdi))
            {
                ModelState.AddModelError(string.Empty, "Lütfen belge adı giriniz.");
                await OnGetAsync(id);
                return Page();
            }

            try
            {
                // Personel klasörünü oluştur
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "ozluk", id.ToString());
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Benzersiz dosya adı oluştur
                var uniqueFileName = $"{Guid.NewGuid()}_{UploadedFile.FileName}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Dosyayı kaydet
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await UploadedFile.CopyToAsync(fileStream);
                }

                // Veritabanına kaydet
                var belge = new Belgeler
                {
                    PersonelID = id,
                    BelgeKategoriID = BelgeKategoriID,
                    BelgeAdi = BelgeAdi,
                    DosyaYolu = $"/uploads/ozluk/{id}/{uniqueFileName}",
                    YuklenmeTarihi = DateTime.Now
                };

                _context.Belgeler.Add(belge);
                await _context.SaveChangesAsync();

                return RedirectToPage(new { id = id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Dosya yüklenirken hata oluştu: {ex.Message}");
                await OnGetAsync(id);
                return Page();
            }
        }

        public async Task<IActionResult> OnPostUploadFotografAsync(int id)
        {
            if (ProfilFotografi == null || ProfilFotografi.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "Lütfen bir fotoğraf seçiniz.");
                await OnGetAsync(id);
                return Page();
            }

            // Dosya uzantısını kontrol et (sadece resim formatları)
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(ProfilFotografi.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError(string.Empty, "Lütfen geçerli bir resim dosyası seçiniz (jpg, jpeg, png, gif).");
                await OnGetAsync(id);
                return Page();
            }

            try
            {
                // Profil fotoğrafları klasörünü oluştur
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "profil", id.ToString());
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Eski fotoğrafı sil (varsa)
                var personel = await _context.Personeller.FindAsync(id);
                if (personel != null && !string.IsNullOrEmpty(personel.FotografDosyaYolu))
                {
                    var oldFilePath = Path.Combine(_environment.WebRootPath, personel.FotografDosyaYolu.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                // Benzersiz dosya adı oluştur
                var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Dosyayı kaydet
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await ProfilFotografi.CopyToAsync(fileStream);
                }

                // Veritabanında FotografDosyaYolu'nu güncelle
                if (personel != null)
                {
                    personel.FotografDosyaYolu = $"/uploads/profil/{id}/{uniqueFileName}";
                    await _context.SaveChangesAsync();
                }

                return RedirectToPage(new { id = id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Fotoğraf yüklenirken hata oluştu: {ex.Message}");
                await OnGetAsync(id);
                return Page();
            }
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
