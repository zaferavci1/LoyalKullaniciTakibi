using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using LoyalKullaniciTakip.Data;

namespace LoyalKullaniciTakip.Pages.Izin
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IzinTalepleri IzinTalebi { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int talepId)
        {
            var talep = await _context.IzinTalepleri
                .Include(i => i.Personel)
                .Include(i => i.IzinTipi)
                .Include(i => i.OnaylayanPersonel)
                .FirstOrDefaultAsync(i => i.TalepID == talepId);

            if (talep == null)
            {
                return NotFound();
            }

            IzinTalebi = talep;
            return Page();
        }

        public async Task<IActionResult> OnPostOnaylaAsync(int talepId)
        {
            // Kritik İş Akışı: Transaction ile Onay ve Puantaj Entegrasyonu
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // a. İzin talebini bul
                    var talep = await _context.IzinTalepleri
                        .Include(i => i.IzinTipi)
                        .FirstOrDefaultAsync(i => i.TalepID == talepId);

                    if (talep == null)
                    {
                        return NotFound();
                    }

                    // Zaten onaylanmış veya reddedilmiş mi kontrol et
                    if (talep.OnayDurumu != 0)
                    {
                        TempData["ErrorMessage"] = "Bu talep zaten işleme alınmış.";
                        return RedirectToPage("./Details", new { talepId });
                    }

                    // b. Onay durumunu güncelle
                    talep.OnayDurumu = 1; // Onaylandı
                    talep.OnaylayanPersonelID = 1; // TODO: Gerçek oturum açmış kullanıcı ID'si kullanılmalı

                    // c. İzin türü için uygun puantaj durumunu bul
                    // Örnek: "Yıllık İzin" kodlu puantaj durumunu bulalım
                    var puantajDurum = await _context.Lookup_PuantajDurumlari
                        .FirstOrDefaultAsync(p => p.Kod == "Yİ" || p.Tanim.Contains("İzin"));

                    if (puantajDurum == null)
                    {
                        // Eğer uygun puantaj durumu yoksa, ilk kayıt olarak kabul edelim
                        puantajDurum = await _context.Lookup_PuantajDurumlari.FirstOrDefaultAsync();
                        
                        if (puantajDurum == null)
                        {
                            throw new InvalidOperationException("Sistemde puantaj durumu tanımlı değil. Lütfen önce puantaj durumlarını tanımlayın.");
                        }
                    }

                    // d. Başlangıç ve bitiş tarihleri arasındaki her gün için döngü
                    var mevcutTarih = talep.BaslangicTarihi.Date;
                    var bitisTarihi = talep.BitisTarihi.Date;

                    while (mevcutTarih <= bitisTarihi)
                    {
                        // Her tarih için puantaj kaydı ekle veya güncelle (UPSERT)
                        var mevcutKayit = await _context.PuantajGunluk
                            .FirstOrDefaultAsync(p => p.PersonelID == talep.PersonelID && p.Tarih == mevcutTarih);

                        if (mevcutKayit == null)
                        {
                            // Yeni kayıt ekle
                            var yeniPuantaj = new PuantajGunluk
                            {
                                PersonelID = talep.PersonelID,
                                Tarih = mevcutTarih,
                                PuantajDurumID = puantajDurum.PuantajDurumID,
                                MesaiSaati = null,
                                Aciklama = $"İzin Talebi #{talep.TalepID} - {talep.IzinTipi.Tanim}"
                            };
                            await _context.PuantajGunluk.AddAsync(yeniPuantaj);
                        }
                        else
                        {
                            // Mevcut kaydı güncelle
                            mevcutKayit.PuantajDurumID = puantajDurum.PuantajDurumID;
                            mevcutKayit.Aciklama = $"İzin Talebi #{talep.TalepID} - {talep.IzinTipi.Tanim} (Güncellendi)";
                        }

                        mevcutTarih = mevcutTarih.AddDays(1);
                    }

                    // e. SaveChanges ve Transaction Commit
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    TempData["SuccessMessage"] = "İzin talebi başarıyla onaylandı ve puantaj kayıtları oluşturuldu.";
                    return RedirectToPage("./List");
                }
                catch (Exception ex)
                {
                    // Hata durumunda Rollback
                    await transaction.RollbackAsync();
                    TempData["ErrorMessage"] = $"İzin onaylanırken hata oluştu: {ex.Message}";
                    return RedirectToPage("./Details", new { talepId });
                }
            }
        }

        public async Task<IActionResult> OnPostReddetAsync(int talepId)
        {
            try
            {
                var talep = await _context.IzinTalepleri
                    .FirstOrDefaultAsync(i => i.TalepID == talepId);

                if (talep == null)
                {
                    return NotFound();
                }

                // Zaten onaylanmış veya reddedilmiş mi kontrol et
                if (talep.OnayDurumu != 0)
                {
                    TempData["ErrorMessage"] = "Bu talep zaten işleme alınmış.";
                    return RedirectToPage("./Details", new { talepId });
                }

                talep.OnayDurumu = 2; // Reddedildi
                talep.OnaylayanPersonelID = 1; // TODO: Gerçek oturum açmış kullanıcı ID'si kullanılmalı

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "İzin talebi reddedildi.";
                return RedirectToPage("./List");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"İzin reddedilirken hata oluştu: {ex.Message}";
                return RedirectToPage("./Details", new { talepId });
            }
        }
    }
}

