using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LoyalKullaniciTakip.Data;
using System.ComponentModel.DataAnnotations;

namespace LoyalKullaniciTakip.Pages.Izin
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
        [Required(ErrorMessage = "Personel seçimi zorunludur")]
        public int SelectedPersonelID { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "İzin tipi zorunludur")]
        public int IzinTipiID { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Başlangıç tarihi zorunludur")]
        public DateTime BaslangicTarihi { get; set; } = DateTime.Now;

        [BindProperty]
        [Required(ErrorMessage = "Bitiş tarihi zorunludur")]
        public DateTime BitisTarihi { get; set; } = DateTime.Now.AddDays(1);

        [BindProperty]
        public string Aciklama { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(int? personelId)
        {
            if (personelId.HasValue)
            {
                PersonelID = personelId.Value;
                SelectedPersonelID = personelId.Value;
            }

            await LoadDropdownsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            PersonelID = SelectedPersonelID;

            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync();
                return Page();
            }

            // Tarih validasyonu
            if (BitisTarihi < BaslangicTarihi)
            {
                ModelState.AddModelError(string.Empty, "Bitiş tarihi başlangıç tarihinden önce olamaz.");
                await LoadDropdownsAsync();
                return Page();
            }

            var izinTalebi = new IzinTalepleri
            {
                PersonelID = SelectedPersonelID,
                IzinTipiID = IzinTipiID,
                BaslangicTarihi = BaslangicTarihi.Date,
                BitisTarihi = BitisTarihi.Date,
                TalepTarihi = DateTime.Now,
                Aciklama = Aciklama,
                OnayDurumu = 0, // Bekliyor
                OnaylayanPersonelID = null
            };

            _context.IzinTalepleri.Add(izinTalebi);
            await _context.SaveChangesAsync();

            return RedirectToPage("./List");
        }

        private async Task LoadDropdownsAsync()
        {
            ViewData["PersonelList"] = new SelectList(
                await _context.Personeller
                    .OrderBy(p => p.Ad)
                    .ThenBy(p => p.Soyad)
                    .Select(p => new { p.PersonelID, AdSoyad = p.Ad + " " + p.Soyad })
                    .ToListAsync(),
                "PersonelID",
                "AdSoyad");

            ViewData["IzinTipiList"] = new SelectList(
                await _context.Lookup_IzinTipleri.ToListAsync(),
                "IzinTipiID",
                "Tanim");
        }
    }
}

