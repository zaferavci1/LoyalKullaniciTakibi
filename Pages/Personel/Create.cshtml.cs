using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LoyalKullaniciTakip.Data;

namespace LoyalKullaniciTakip.Pages.Personel
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public CreatePersonelViewModel ViewModel { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadLookupsAsync();
            return Page();
        }

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

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadLookupsAsync();
                return Page();
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Personel nesnesini oluştur
                var personel = new Data.Personel
                {
                    Ad = ViewModel.Ad,
                    Soyad = ViewModel.Soyad,
                    TCKimlikNo = ViewModel.TCKimlikNo,
                    DogumTarihi = ViewModel.DogumTarihi,
                    Cinsiyet = ViewModel.Cinsiyet,
                    DepartmanID = ViewModel.DepartmanID,
                    MeslekID = ViewModel.MeslekID
                };

                _context.Personeller.Add(personel);
                await _context.SaveChangesAsync(); // PersonelID otomatik oluşur

                // 2. SGK Detay bilgilerini oluştur
                var sgkDetay = new Personel_Detay_SGK
                {
                    PersonelID = personel.PersonelID,
                    IseGirisTarihi = ViewModel.IseGirisTarihi,
                    CalismaTipiID = ViewModel.CalismaTipiID,
                    SGKMeslekKoduID = ViewModel.SGKMeslekKoduID,
                    IsyeriSicilNo = ViewModel.IsyeriSicilNo,
                    Personel = personel
                };

                _context.Personel_Detay_SGK.Add(sgkDetay);

                // 3. Muhasebe Detay bilgilerini oluştur
                var muhasebeDetay = new Personel_Detay_Muhasebe
                {
                    PersonelID = personel.PersonelID,
                    TemelMaas = ViewModel.TemelMaas,
                    MaasTipi = ViewModel.MaasTipi,
                    IBAN = ViewModel.IBAN,
                    Personel = personel
                };

                _context.Personel_Detay_Muhasebe.Add(muhasebeDetay);

                await _context.SaveChangesAsync();

                // 4. Transaction'ı commit et
                await transaction.CommitAsync();

                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                // Hata durumunda rollback yap
                await transaction.RollbackAsync();

                // Inner exception'ları da göster
                var errorMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMessage += " | Inner: " + ex.InnerException.Message;
                    if (ex.InnerException.InnerException != null)
                    {
                        errorMessage += " | Inner2: " + ex.InnerException.InnerException.Message;
                    }
                }

                ModelState.AddModelError(string.Empty,
                    $"Personel kaydı sırasında bir hata oluştu: {errorMessage}");

                await LoadLookupsAsync();
                return Page();
            }
        }
    }
}
