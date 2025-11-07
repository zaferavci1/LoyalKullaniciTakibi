using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LoyalKullaniciTakip.Data;
using System.Globalization;

namespace LoyalKullaniciTakip.Pages.Raporlar
{
    public class PuantajOzetModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public PuantajOzetModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public int SecilenAy { get; set; }

        [BindProperty]
        public int SecilenYil { get; set; }

        public List<PuantajOzetViewModel> Rapor { get; set; } = new List<PuantajOzetViewModel>();
        public List<SelectListItem> AyListesi { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> YilListesi { get; set; } = new List<SelectListItem>();
        public bool RaporGosterilsin { get; set; } = false;

        public void OnGet()
        {
            // Varsayılan olarak bu ayı seç
            SecilenAy = DateTime.Today.Month;
            SecilenYil = DateTime.Today.Year;

            PrepareDropdowns();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            PrepareDropdowns();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            RaporGosterilsin = true;

            // Kritik LINQ Sorgusu: GroupBy ile Puantaj Özeti
            Rapor = await _context.PuantajGunluk
                .Include(p => p.Personel)
                .Include(p => p.PuantajDurum)
                .Where(p => p.Tarih.Year == SecilenYil && p.Tarih.Month == SecilenAy)
                .GroupBy(p => new { p.PersonelID, p.Personel.Ad, p.Personel.Soyad })
                .Select(g => new PuantajOzetViewModel
                {
                    PersonelAdSoyad = g.Key.Ad + " " + g.Key.Soyad,
                    CalisilanGunSayisi = g.Count(x => x.PuantajDurumID == 1), // Çalıştı
                    RaporluGunSayisi = g.Count(x => x.PuantajDurumID == 2), // Raporlu
                    YillikIzinGunSayisi = g.Count(x => x.PuantajDurumID == 3), // Yıllık İzin
                    MazeretIzniGunSayisi = g.Count(x => x.PuantajDurumID == 4), // Mazeret İzni
                    UcretsizIzinGunSayisi = g.Count(x => x.PuantajDurumID == 5), // Ücretsiz İzin
                    TatilGunSayisi = g.Count(x => x.PuantajDurumID == 7 || x.PuantajDurumID == 8 || x.PuantajDurumID == 9 || x.PuantajDurumID == 11), // Tatil, CT, Pazar, Genel Tatil
                    FazlaMesaiGunSayisi = g.Count(x => x.PuantajDurumID == 10), // Fazla Mesai
                    ToplamMesaiSaati = g.Sum(x => x.MesaiSaati ?? 0),
                    ToplamGunSayisi = g.Count()
                })
                .OrderBy(r => r.PersonelAdSoyad)
                .ToListAsync();

            return Page();
        }

        private void PrepareDropdowns()
        {
            // Aylar listesi (Türkçe)
            var turkishCulture = new CultureInfo("tr-TR");
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
        }
    }

    /// <summary>
    /// Puantaj özet raporu için ViewModel
    /// </summary>
    public class PuantajOzetViewModel
    {
        public string PersonelAdSoyad { get; set; } = string.Empty;
        public int CalisilanGunSayisi { get; set; }
        public int RaporluGunSayisi { get; set; }
        public int YillikIzinGunSayisi { get; set; }
        public int MazeretIzniGunSayisi { get; set; }
        public int UcretsizIzinGunSayisi { get; set; }
        public int TatilGunSayisi { get; set; }
        public int FazlaMesaiGunSayisi { get; set; }
        public decimal ToplamMesaiSaati { get; set; }
        public int ToplamGunSayisi { get; set; }
    }
}

