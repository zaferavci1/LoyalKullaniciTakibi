using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using LoyalKullaniciTakip.Data;

namespace LoyalKullaniciTakip.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly ApplicationDbContext _context;

    public IndexModel(ILogger<IndexModel> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    // Dashboard Verileri
    public List<PersonelIzinDto> BugunIzinliOlanlar { get; set; } = new List<PersonelIzinDto>();
    public List<PersonelDogumGunuDto> YaklasanDogumGunleri { get; set; } = new List<PersonelDogumGunuDto>();
    public int ToplamPersonelSayisi { get; set; }
    public int BekleyenIzinTalepSayisi { get; set; }
    public int BuAyIzinliGunSayisi { get; set; }
    public int BugunDevamsizSayisi { get; set; }
    public int BugunRaporluSayisi { get; set; }
    public int AktifCalisanSayisi { get; set; }

    public async Task OnGetAsync()
    {
        var bugun = DateTime.Today;

        // 1. Bugün İzinli Olanlar
        BugunIzinliOlanlar = await _context.IzinTalepleri
            .Include(i => i.Personel)
            .Include(i => i.IzinTipi)
            .Where(i => i.OnayDurumu == 1 && 
                       i.BaslangicTarihi.Date <= bugun && 
                       i.BitisTarihi.Date >= bugun)
            .Select(i => new PersonelIzinDto
            {
                PersonelAdSoyad = i.Personel.Ad + " " + i.Personel.Soyad,
                IzinTipi = i.IzinTipi.Tanim,
                BaslangicTarihi = i.BaslangicTarihi,
                BitisTarihi = i.BitisTarihi
            })
            .ToListAsync();

        // 2. Yaklaşan Doğum Günleri (Bu Ay)
        var bugunAy = bugun.Month;
        var bugunGun = bugun.Day;
        
        YaklasanDogumGunleri = await _context.Personeller
            .Where(p => p.DogumTarihi.Month == bugunAy && p.DogumTarihi.Day >= bugunGun)
            .OrderBy(p => p.DogumTarihi.Day)
            .Select(p => new PersonelDogumGunuDto
            {
                PersonelAdSoyad = p.Ad + " " + p.Soyad,
                DogumTarihi = p.DogumTarihi,
                KacGunSonra = p.DogumTarihi.Day - bugunGun
            })
            .Take(10)
            .ToListAsync();

        // 3. Genel İstatistikler
        ToplamPersonelSayisi = await _context.Personeller.CountAsync();
        BekleyenIzinTalepSayisi = await _context.IzinTalepleri
            .CountAsync(i => i.OnayDurumu == 0);

        var ayinIlkGunu = new DateTime(bugun.Year, bugun.Month, 1);
        var ayinSonGunu = ayinIlkGunu.AddMonths(1).AddDays(-1);
        
        BuAyIzinliGunSayisi = await _context.IzinTalepleri
            .Where(i => i.OnayDurumu == 1 &&
                       ((i.BaslangicTarihi >= ayinIlkGunu && i.BaslangicTarihi <= ayinSonGunu) ||
                        (i.BitisTarihi >= ayinIlkGunu && i.BitisTarihi <= ayinSonGunu)))
            .SumAsync(i => (i.BitisTarihi.Date - i.BaslangicTarihi.Date).Days + 1);

        // 4. Bugün Devamsız ve Raporlu Sayıları
        var devamsizDurum = await _context.Lookup_PuantajDurumlari
            .FirstOrDefaultAsync(p => p.Tanim.Contains("Devamsız") || p.Kod == "D");
        var raporluDurum = await _context.Lookup_PuantajDurumlari
            .FirstOrDefaultAsync(p => p.Tanim.Contains("Raporlu") || p.Kod == "R");

        if (devamsizDurum != null)
        {
            BugunDevamsizSayisi = await _context.PuantajGunluk
                .CountAsync(p => p.Tarih == bugun && p.PuantajDurumID == devamsizDurum.PuantajDurumID);
        }

        if (raporluDurum != null)
        {
            BugunRaporluSayisi = await _context.PuantajGunluk
                .CountAsync(p => p.Tarih == bugun && p.PuantajDurumID == raporluDurum.PuantajDurumID);
        }

        // 5. Aktif Çalışan Sayısı = Toplam - İzinli - Devamsız - Raporlu
        AktifCalisanSayisi = ToplamPersonelSayisi - BugunIzinliOlanlar.Count - BugunDevamsizSayisi - BugunRaporluSayisi;
    }
}

// DTO sınıfları
public class PersonelIzinDto
{
    public string PersonelAdSoyad { get; set; } = string.Empty;
    public string IzinTipi { get; set; } = string.Empty;
    public DateTime BaslangicTarihi { get; set; }
    public DateTime BitisTarihi { get; set; }
}

public class PersonelDogumGunuDto
{
    public string PersonelAdSoyad { get; set; } = string.Empty;
    public DateTime DogumTarihi { get; set; }
    public int KacGunSonra { get; set; }
}
