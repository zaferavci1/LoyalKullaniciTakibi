using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LoyalKullaniciTakip.Data;

namespace LoyalKullaniciTakip.Pages.Personel
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public CreatePersonelViewModel ViewModel { get; set; } = default!;

        public int PersonelID { get; set; }

        private async Task LoadLookupsAsync()
        {
            ViewData["CalismaTipiList"] = new SelectList(
                await _context.Lookup_CalismaTipi.ToListAsync(),
                "CalismaTipiID",
                "Tanim");

            ViewData["MeslekKodlariList"] = new SelectList(
                await _context.Lookup_MeslekKodlari.ToListAsync(),
                "MeslekKoduID",
                "Tanim");

            ViewData["DepartmanlarList"] = new SelectList(
                await _context.Lookup_Departmanlar.ToListAsync(),
                "DepartmanID",
                "Tanim");

            ViewData["MesleklerList"] = new SelectList(
                await _context.Lookup_Meslekler.ToListAsync(),
                "MeslekID",
                "Tanim");
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            PersonelID = id;

            var personel = await _context.Personeller
                .Include(p => p.Personel_Detay_SGK)
                .Include(p => p.Personel_Detay_Muhasebe)
                .FirstOrDefaultAsync(p => p.PersonelID == id);

            if (personel == null)
            {
                return NotFound();
            }

            // Entity'den ViewModel'e veri transferi
            ViewModel = new CreatePersonelViewModel
            {
                // Personel Bilgileri
                Ad = personel.Ad,
                Soyad = personel.Soyad,
                TCKimlikNo = personel.TCKimlikNo,
                DogumTarihi = personel.DogumTarihi,
                Cinsiyet = personel.Cinsiyet,
                DepartmanID = personel.DepartmanID,
                MeslekID = personel.MeslekID,

                // SGK Detay Bilgileri
                IseGirisTarihi = personel.Personel_Detay_SGK?.IseGirisTarihi ?? DateTime.Now,
                CalismaTipiID = personel.Personel_Detay_SGK?.CalismaTipiID ?? 0,
                SGKMeslekKoduID = personel.Personel_Detay_SGK?.SGKMeslekKoduID ?? 0,
                IsyeriSicilNo = personel.Personel_Detay_SGK?.IsyeriSicilNo ?? string.Empty,

                // Muhasebe Detay Bilgileri
                TemelMaas = personel.Personel_Detay_Muhasebe?.TemelMaas ?? 0,
                MaasTipi = personel.Personel_Detay_Muhasebe?.MaasTipi ?? string.Empty,
                IBAN = personel.Personel_Detay_Muhasebe?.IBAN ?? string.Empty
            };

            await LoadLookupsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            PersonelID = id;

            if (!ModelState.IsValid)
            {
                await LoadLookupsAsync();
                return Page();
            }

            try
            {
                // Mevcut personel ve detaylarını yükle
                var personel = await _context.Personeller
                    .Include(p => p.Personel_Detay_SGK)
                    .Include(p => p.Personel_Detay_Muhasebe)
                    .FirstOrDefaultAsync(p => p.PersonelID == id);

                if (personel == null)
                {
                    return NotFound();
                }

                // 1. Personel bilgilerini güncelle
                personel.Ad = ViewModel.Ad;
                personel.Soyad = ViewModel.Soyad;
                personel.TCKimlikNo = ViewModel.TCKimlikNo;
                personel.DogumTarihi = ViewModel.DogumTarihi;
                personel.Cinsiyet = ViewModel.Cinsiyet;
                personel.DepartmanID = ViewModel.DepartmanID;
                personel.MeslekID = ViewModel.MeslekID;

                // 2. SGK Detay bilgilerini güncelle
                if (personel.Personel_Detay_SGK != null)
                {
                    personel.Personel_Detay_SGK.IseGirisTarihi = ViewModel.IseGirisTarihi;
                    personel.Personel_Detay_SGK.CalismaTipiID = ViewModel.CalismaTipiID;
                    personel.Personel_Detay_SGK.SGKMeslekKoduID = ViewModel.SGKMeslekKoduID;
                    personel.Personel_Detay_SGK.IsyeriSicilNo = ViewModel.IsyeriSicilNo;
                }

                // 3. Muhasebe Detay bilgilerini güncelle
                if (personel.Personel_Detay_Muhasebe != null)
                {
                    personel.Personel_Detay_Muhasebe.TemelMaas = ViewModel.TemelMaas;
                    personel.Personel_Detay_Muhasebe.MaasTipi = ViewModel.MaasTipi;
                    personel.Personel_Detay_Muhasebe.IBAN = ViewModel.IBAN;
                }

                await _context.SaveChangesAsync();

                return RedirectToPage("./Index");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await PersonelExistsAsync(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty,
                    $"Güncelleme sırasında bir hata oluştu: {ex.Message}");
                await LoadLookupsAsync();
                return Page();
            }
        }

        private async Task<bool> PersonelExistsAsync(int id)
        {
            return await _context.Personeller.AnyAsync(p => p.PersonelID == id);
        }
    }
}
