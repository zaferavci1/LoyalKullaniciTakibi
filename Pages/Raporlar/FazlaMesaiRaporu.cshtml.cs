using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LoyalKullaniciTakip.Data;
using System.Globalization;

namespace LoyalKullaniciTakip.Pages.Raporlar
{
    public class FazlaMesaiRaporuModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public FazlaMesaiRaporuModel(ApplicationDbContext context)
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

        public List<FazlaMesaiRaporuViewModel> Rapor { get; set; } = new List<FazlaMesaiRaporuViewModel>();
        public List<SelectListItem> AyListesi { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> YilListesi { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> DepartmanListesi { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> MeslekListesi { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> PersonelListesi { get; set; } = new List<SelectListItem>();
        public bool RaporGosterilsin { get; set; } = false;

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

            // Günlük çalışma saatini al
            var gunlukCalismaSaatiAyar = await _context.Lookup_GenelAyarlar
                .FirstOrDefaultAsync(a => a.AyarKey == "GunlukCalismaSaati");
            var gunlukCalismaSaati = decimal.Parse(
                gunlukCalismaSaatiAyar?.AyarValue ?? "7.5",
                CultureInfo.InvariantCulture
            );

            // Seçilen ay için tarih aralığı
            var ayinIlkGunu = new DateTime(SecilenYil, SecilenAy, 1);
            var ayinSonGunu = ayinIlkGunu.AddMonths(1).AddDays(-1);

            // Puantaj kayıtlarını çek
            var query = _context.PuantajGunluk
                .Include(p => p.Personel)
                .Include(p => p.PuantajDurum)
                .Where(p => p.Tarih >= ayinIlkGunu && p.Tarih <= ayinSonGunu);

            // Departman filtresi varsa uygula
            if (SecilenDepartmanID.HasValue && SecilenDepartmanID.Value > 0)
            {
                query = query.Where(p => p.Personel.DepartmanID == SecilenDepartmanID.Value);
            }

            // Meslek filtresi varsa uygula
            if (SecilenMeslekID.HasValue && SecilenMeslekID.Value > 0)
            {
                query = query.Where(p => p.Personel.MeslekID == SecilenMeslekID.Value);
            }

            // Personel filtresi varsa uygula
            if (SecilenPersonelID.HasValue && SecilenPersonelID.Value > 0)
            {
                query = query.Where(p => p.PersonelID == SecilenPersonelID.Value);
            }

            var puantajKayitlari = await query.ToListAsync();

            // Personel bazında grupla
            var personelGruplari = puantajKayitlari
                .GroupBy(p => new { p.PersonelID, p.Personel.Ad, p.Personel.Soyad });

            foreach (var grup in personelGruplari)
            {
                decimal haftaIciFM = 0;
                decimal cumartesiFM = 0;
                decimal pazarFM = 0;
                int fazlaMesaiGunSayisi = 0;

                var detaylar = new List<FazlaMesaiDetayViewModel>();

                foreach (var kayit in grup.OrderBy(k => k.Tarih))
                {
                    var toplamMesai = kayit.MesaiSaati ?? 0;

                    // Fazla mesai var mı kontrol et
                    if (toplamMesai > gunlukCalismaSaati)
                    {
                        var fazlaMesai = toplamMesai - gunlukCalismaSaati;
                        fazlaMesaiGunSayisi++;

                        string fmTipi;
                        if (kayit.Tarih.DayOfWeek == DayOfWeek.Sunday)
                        {
                            pazarFM += fazlaMesai;
                            fmTipi = "Pazar/Tatil";
                        }
                        else if (kayit.Tarih.DayOfWeek == DayOfWeek.Saturday)
                        {
                            cumartesiFM += fazlaMesai;
                            fmTipi = "Cumartesi";
                        }
                        else // Pazartesi-Cuma
                        {
                            haftaIciFM += fazlaMesai;
                            fmTipi = "Hafta İçi";
                        }

                        detaylar.Add(new FazlaMesaiDetayViewModel
                        {
                            Tarih = kayit.Tarih,
                            Gun = kayit.Tarih.ToString("dddd", new CultureInfo("tr-TR")),
                            PuantajDurum = kayit.PuantajDurum?.Tanim ?? "-",
                            ToplamMesaiSaati = toplamMesai,
                            NormalMesai = gunlukCalismaSaati,
                            FazlaMesai = fazlaMesai,
                            FazlaMesaiTipi = fmTipi
                        });
                    }
                }

                // Sadece fazla mesai yapan personelleri ekle
                if (fazlaMesaiGunSayisi > 0)
                {
                    Rapor.Add(new FazlaMesaiRaporuViewModel
                    {
                        PersonelAdSoyad = $"{grup.Key.Ad} {grup.Key.Soyad}",
                        ToplamFazlaMesaiSaati = haftaIciFM + cumartesiFM + pazarFM,
                        HaftaIciFazlaMesai = haftaIciFM,
                        CumartesiFazlaMesai = cumartesiFM,
                        PazarTatilFazlaMesai = pazarFM,
                        FazlaMesaiGunSayisi = fazlaMesaiGunSayisi,
                        Detaylar = detaylar
                    });
                }
            }

            // Toplam fazla mesai saatine göre sırala (azalan)
            Rapor = Rapor.OrderByDescending(r => r.ToplamFazlaMesaiSaati).ToList();

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

    /// <summary>
    /// Fazla mesai özet raporu ViewModel
    /// </summary>
    public class FazlaMesaiRaporuViewModel
    {
        public string PersonelAdSoyad { get; set; } = string.Empty;
        public decimal ToplamFazlaMesaiSaati { get; set; }
        public decimal HaftaIciFazlaMesai { get; set; }
        public decimal CumartesiFazlaMesai { get; set; }
        public decimal PazarTatilFazlaMesai { get; set; }
        public int FazlaMesaiGunSayisi { get; set; }
        public List<FazlaMesaiDetayViewModel> Detaylar { get; set; } = new List<FazlaMesaiDetayViewModel>();
    }

    /// <summary>
    /// Fazla mesai detay ViewModel (tarih bazlı)
    /// </summary>
    public class FazlaMesaiDetayViewModel
    {
        public DateTime Tarih { get; set; }
        public string Gun { get; set; } = string.Empty;
        public string PuantajDurum { get; set; } = string.Empty;
        public decimal ToplamMesaiSaati { get; set; }
        public decimal NormalMesai { get; set; }
        public decimal FazlaMesai { get; set; }
        public string FazlaMesaiTipi { get; set; } = string.Empty;
    }
}
