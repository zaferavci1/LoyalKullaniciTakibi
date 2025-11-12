using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LoyalKullaniciTakip.Data;
using LoyalKullaniciTakip.Data.Lookups;
using System.Globalization;

namespace LoyalKullaniciTakip.Pages.Raporlar
{
    public class DepartmanBazliPuantajModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DepartmanBazliPuantajModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public int SecilenAy { get; set; }

        [BindProperty]
        public int SecilenYil { get; set; }

        [BindProperty]
        public int? SecilenDepartmanID { get; set; }

        public List<SelectListItem> AyListesi { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> YilListesi { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> DepartmanListesi { get; set; } = new List<SelectListItem>();
        public bool RaporGosterilsin { get; set; } = false;

        // Rapor verileri
        public DepartmanPuantajOzet? Ozet { get; set; }
        public List<DepartmanPersonelDetay> PersonelDetaylari { get; set; } = new List<DepartmanPersonelDetay>();

        public async Task OnGetAsync()
        {
            // Varsayılan olarak bu ayı seç
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

            // Seçilen dönemin ilk ve son günleri
            var ayinIlkGunu = new DateTime(SecilenYil, SecilenAy, 1);
            var ayinSonGunu = ayinIlkGunu.AddMonths(1).AddDays(-1);

            // Departman filtresi
            IQueryable<PuantajGunluk> query = _context.PuantajGunluk
                .Include(p => p.Personel)
                .Include(p => p.Departman)
                .Include(p => p.PuantajDurum)
                .Where(p => p.Tarih >= ayinIlkGunu && p.Tarih <= ayinSonGunu);

            if (SecilenDepartmanID.HasValue && SecilenDepartmanID.Value > 0)
            {
                query = query.Where(p => p.DepartmanID == SecilenDepartmanID.Value);
            }

            var puantajKayitlari = await query.ToListAsync();

            if (puantajKayitlari.Any())
            {
                // Özet istatistikler
                var toplamGunSayisi = puantajKayitlari.Count;
                var benzersizPersonelSayisi = puantajKayitlari.Select(p => p.PersonelID).Distinct().Count();
                var toplamMesaiSaati = puantajKayitlari.Sum(p => p.MesaiSaati ?? 0);
                var ortalamaMesaiSaati = benzersizPersonelSayisi > 0 ? toplamMesaiSaati / benzersizPersonelSayisi : 0;

                // Durum bazlı istatistikler
                var durumGruplari = puantajKayitlari
                    .GroupBy(p => p.PuantajDurum?.Tanim ?? "Bilinmeyen")
                    .Select(g => new DurumIstatistik
                    {
                        DurumAdi = g.Key,
                        GunSayisi = g.Count()
                    })
                    .OrderByDescending(d => d.GunSayisi)
                    .ToList();

                Ozet = new DepartmanPuantajOzet
                {
                    ToplamGunSayisi = toplamGunSayisi,
                    BenzersizPersonelSayisi = benzersizPersonelSayisi,
                    ToplamMesaiSaati = toplamMesaiSaati,
                    OrtalamaMesaiSaati = ortalamaMesaiSaati,
                    DurumIstatistikleri = durumGruplari
                };

                // Personel bazlı detaylar
                PersonelDetaylari = puantajKayitlari
                    .GroupBy(p => new
                    {
                        p.PersonelID,
                        PersonelAd = p.Personel?.Ad ?? "",
                        PersonelSoyad = p.Personel?.Soyad ?? "",
                        DepartmanID = p.DepartmanID,
                        DepartmanAdi = p.Departman?.Tanim ?? "Departman Belirtilmemiş"
                    })
                    .Select(g => new DepartmanPersonelDetay
                    {
                        PersonelID = g.Key.PersonelID,
                        PersonelAdSoyad = $"{g.Key.PersonelAd} {g.Key.PersonelSoyad}",
                        DepartmanAdi = g.Key.DepartmanAdi,
                        CalisilanGunSayisi = g.Count(),
                        ToplamMesaiSaati = g.Sum(x => x.MesaiSaati ?? 0),
                        OrtalamaMesaiSaati = g.Average(x => x.MesaiSaati ?? 0)
                    })
                    .OrderBy(p => p.PersonelAdSoyad)
                    .ToList();
            }

            return Page();
        }

        private async Task PrepareDropdownsAsync()
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

            // Departmanlar listesi
            var departmanlar = await _context.Lookup_Departmanlar
                .OrderBy(d => d.Tanim)
                .ToListAsync();

            DepartmanListesi = new List<SelectListItem>
            {
                new SelectListItem { Value = "0", Text = "Tüm Departmanlar", Selected = !SecilenDepartmanID.HasValue || SecilenDepartmanID.Value == 0 }
            };

            DepartmanListesi.AddRange(departmanlar.Select(d => new SelectListItem
            {
                Value = d.DepartmanID.ToString(),
                Text = d.Tanim,
                Selected = SecilenDepartmanID == d.DepartmanID
            }));
        }
    }

    public class DepartmanPuantajOzet
    {
        public int ToplamGunSayisi { get; set; }
        public int BenzersizPersonelSayisi { get; set; }
        public decimal ToplamMesaiSaati { get; set; }
        public decimal OrtalamaMesaiSaati { get; set; }
        public List<DurumIstatistik> DurumIstatistikleri { get; set; } = new List<DurumIstatistik>();
    }

    public class DurumIstatistik
    {
        public string DurumAdi { get; set; } = string.Empty;
        public int GunSayisi { get; set; }
    }

    public class DepartmanPersonelDetay
    {
        public int PersonelID { get; set; }
        public string PersonelAdSoyad { get; set; } = string.Empty;
        public string DepartmanAdi { get; set; } = string.Empty;
        public int CalisilanGunSayisi { get; set; }
        public decimal ToplamMesaiSaati { get; set; }
        public decimal OrtalamaMesaiSaati { get; set; }
    }
}
