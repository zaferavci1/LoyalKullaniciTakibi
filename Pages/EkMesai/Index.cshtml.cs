using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LoyalKullaniciTakip.Data;

namespace LoyalKullaniciTakip.Pages.EkMesai
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public int SecilenAy { get; set; }

        [BindProperty]
        public int SecilenYil { get; set; }

        [BindProperty]
        public int? SecilenDepartmanID { get; set; }

        [BindProperty]
        public int? SecilenMeslekID { get; set; }

        [BindProperty]
        public int? SecilenPersonelID { get; set; }

        public List<EkMesaiViewModel> EkMesaiListesi { get; set; } = new List<EkMesaiViewModel>();
        public List<SelectListItem> AyListesi { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> YilListesi { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> DepartmanListesi { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> MeslekListesi { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> PersonelListesi { get; set; } = new List<SelectListItem>();
        public bool RaporGosterilsin { get; set; } = false;

        // Özet istatistikler
        public int ToplamKayitSayisi { get; set; }
        public decimal ToplamEkMesaiSaati { get; set; }
        public decimal ToplamEkMesaiTutari { get; set; }

        public async Task OnGetAsync()
        {
            SecilenAy = DateTime.Today.Month;
            SecilenYil = DateTime.Today.Year;

            await PrepareDropdownsAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await PrepareDropdownsAsync();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            RaporGosterilsin = true;

            // Seçilen ay için tarih aralığı
            var ayinIlkGunu = new DateTime(SecilenYil, SecilenAy, 1);
            var ayinSonGunu = ayinIlkGunu.AddMonths(1).AddDays(-1);

            // Ek mesai kayıtlarını çek
            var query = _context.EkMesaiKayitlari
                .Include(e => e.Personel)
                    .ThenInclude(p => p.Departman)
                .Include(e => e.Personel)
                    .ThenInclude(p => p.Meslek)
                .Where(e => e.Tarih >= ayinIlkGunu && e.Tarih <= ayinSonGunu);

            // Departman filtresi
            if (SecilenDepartmanID.HasValue && SecilenDepartmanID.Value > 0)
            {
                query = query.Where(e => e.Personel.DepartmanID == SecilenDepartmanID.Value);
            }

            // Meslek filtresi
            if (SecilenMeslekID.HasValue && SecilenMeslekID.Value > 0)
            {
                query = query.Where(e => e.Personel.MeslekID == SecilenMeslekID.Value);
            }

            // Personel filtresi
            if (SecilenPersonelID.HasValue && SecilenPersonelID.Value > 0)
            {
                query = query.Where(e => e.PersonelID == SecilenPersonelID.Value);
            }

            var kayitlar = await query.OrderByDescending(e => e.Tarih).ToListAsync();

            EkMesaiListesi = kayitlar.Select(e => new EkMesaiViewModel
            {
                EkMesaiID = e.EkMesaiID,
                PersonelAdSoyad = $"{e.Personel.Ad} {e.Personel.Soyad}",
                Departman = e.Personel.Departman?.Tanim ?? "-",
                Meslek = e.Personel.Meslek?.Tanim ?? "-",
                Tarih = e.Tarih,
                Gun = e.Tarih.ToString("dddd", new System.Globalization.CultureInfo("tr-TR")),
                GunTipi = e.GunTipi,
                EkMesaiSaati = e.EkMesaiSaati,
                Katsayi = e.Katsayi,
                SaatlikUcret = e.SaatlikUcret,
                HesaplananTutar = e.HesaplananTutar,
                Aciklama = e.Aciklama
            }).ToList();

            // Özet istatistikler
            ToplamKayitSayisi = EkMesaiListesi.Count;
            ToplamEkMesaiSaati = EkMesaiListesi.Sum(e => e.EkMesaiSaati);
            ToplamEkMesaiTutari = EkMesaiListesi.Sum(e => e.HesaplananTutar);

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int ekMesaiId)
        {
            var kayit = await _context.EkMesaiKayitlari.FindAsync(ekMesaiId);

            if (kayit != null)
            {
                _context.EkMesaiKayitlari.Remove(kayit);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Ek mesai kaydı başarıyla silindi.";
            }
            else
            {
                TempData["ErrorMessage"] = "Kayıt bulunamadı.";
            }

            return RedirectToPage();
        }

        private async Task PrepareDropdownsAsync()
        {
            // Aylar listesi (Türkçe)
            var turkishCulture = new System.Globalization.CultureInfo("tr-TR");
            AyListesi = Enumerable.Range(1, 12)
                .Select(i => new SelectListItem
                {
                    Value = i.ToString(),
                    Text = turkishCulture.DateTimeFormat.GetMonthName(i),
                    Selected = i == SecilenAy
                })
                .ToList();

            // Yıllar listesi (Son 5 yıl + gelecek 1 yıl)
            var buYil = DateTime.Today.Year;
            YilListesi = Enumerable.Range(buYil - 5, 7)
                .Select(y => new SelectListItem
                {
                    Value = y.ToString(),
                    Text = y.ToString(),
                    Selected = y == SecilenYil
                })
                .OrderByDescending(x => x.Value)
                .ToList();

            // Departman listesi
            var departmanlar = await _context.Lookup_Departmanlar
                .OrderBy(d => d.Tanim)
                .Select(d => new SelectListItem
                {
                    Value = d.DepartmanID.ToString(),
                    Text = d.Tanim,
                    Selected = SecilenDepartmanID.HasValue && SecilenDepartmanID.Value == d.DepartmanID
                })
                .ToListAsync();

            DepartmanListesi = new List<SelectListItem>
            {
                new SelectListItem { Value = "0", Text = "Tüm Departmanlar", Selected = !SecilenDepartmanID.HasValue || SecilenDepartmanID.Value == 0 }
            };
            DepartmanListesi.AddRange(departmanlar);

            // Meslek listesi
            var meslekler = await _context.Lookup_Meslekler
                .OrderBy(m => m.Tanim)
                .Select(m => new SelectListItem
                {
                    Value = m.MeslekID.ToString(),
                    Text = m.Tanim,
                    Selected = SecilenMeslekID.HasValue && SecilenMeslekID.Value == m.MeslekID
                })
                .ToListAsync();

            MeslekListesi = new List<SelectListItem>
            {
                new SelectListItem { Value = "0", Text = "Tüm Meslekler", Selected = !SecilenMeslekID.HasValue || SecilenMeslekID.Value == 0 }
            };
            MeslekListesi.AddRange(meslekler);

            // Personel listesi
            var personeller = await _context.Personeller
                .OrderBy(p => p.Ad)
                .ThenBy(p => p.Soyad)
                .Select(p => new SelectListItem
                {
                    Value = p.PersonelID.ToString(),
                    Text = $"{p.Ad} {p.Soyad}",
                    Selected = SecilenPersonelID.HasValue && SecilenPersonelID.Value == p.PersonelID
                })
                .ToListAsync();

            PersonelListesi = new List<SelectListItem>
            {
                new SelectListItem { Value = "0", Text = "Tüm Personeller", Selected = !SecilenPersonelID.HasValue || SecilenPersonelID.Value == 0 }
            };
            PersonelListesi.AddRange(personeller);
        }
    }

    public class EkMesaiViewModel
    {
        public int EkMesaiID { get; set; }
        public string PersonelAdSoyad { get; set; } = string.Empty;
        public string Departman { get; set; } = string.Empty;
        public string Meslek { get; set; } = string.Empty;
        public DateTime Tarih { get; set; }
        public string Gun { get; set; } = string.Empty;
        public string GunTipi { get; set; } = string.Empty;
        public decimal EkMesaiSaati { get; set; }
        public decimal Katsayi { get; set; }
        public decimal SaatlikUcret { get; set; }
        public decimal HesaplananTutar { get; set; }
        public string Aciklama { get; set; } = string.Empty;
    }
}
