using Microsoft.EntityFrameworkCore;
using LoyalKullaniciTakip.Data;
using System.Globalization;
using Microsoft.AspNetCore.Localization;

// PostgreSQL DateTime Unspecified desteği için
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Kültür ayarları - Model binding için InvariantCulture kullan
var supportedCultures = new[] { new CultureInfo("en-US") };
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("en-US");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.FallBackToParentCultures = false;
    options.FallBackToParentUICultures = false;
});

// Add services to the container.
builder.Services.AddRazorPages();

// Add Authentication and Authorization
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

// Configure DbContext with PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Seed Data - Kıdem İzin Hakediş Kuralları ve Genel Ayarlar
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();

    // Kıdem izin kuralları yoksa seed et
    if (!context.Lookup_KidemIzinHakedis.Any())
    {
        context.Lookup_KidemIzinHakedis.AddRange(
            new LoyalKullaniciTakip.Data.Lookups.Lookup_KidemIzinHakedis { MinKidemYili = 1, MaxKidemYili = 5, HakedilenGunSayisi = 14 },
            new LoyalKullaniciTakip.Data.Lookups.Lookup_KidemIzinHakedis { MinKidemYili = 6, MaxKidemYili = 14, HakedilenGunSayisi = 20 },
            new LoyalKullaniciTakip.Data.Lookups.Lookup_KidemIzinHakedis { MinKidemYili = 15, MaxKidemYili = 99, HakedilenGunSayisi = 26 }
        );
        context.SaveChanges();
    }

    // Genel ayarlar yoksa seed et
    if (!context.Lookup_GenelAyarlar.Any(g => g.AyarKey == "FazlaMesaiCarpani"))
    {
        context.Lookup_GenelAyarlar.Add(
            new LoyalKullaniciTakip.Data.Lookups.Lookup_GenelAyarlar
            {
                AyarKey = "FazlaMesaiCarpani",
                AyarValue = "1.5"
            }
        );
        context.SaveChanges();
    }

    // Hafta içi mesai çarpanı
    if (!context.Lookup_GenelAyarlar.Any(g => g.AyarKey == "HaftaIciMesaiCarpani"))
    {
        context.Lookup_GenelAyarlar.Add(
            new LoyalKullaniciTakip.Data.Lookups.Lookup_GenelAyarlar
            {
                AyarKey = "HaftaIciMesaiCarpani",
                AyarValue = "1.5"
            }
        );
        context.SaveChanges();
    }

    // Cumartesi mesai çarpanı
    if (!context.Lookup_GenelAyarlar.Any(g => g.AyarKey == "CumartesiMesaiCarpani"))
    {
        context.Lookup_GenelAyarlar.Add(
            new LoyalKullaniciTakip.Data.Lookups.Lookup_GenelAyarlar
            {
                AyarKey = "CumartesiMesaiCarpani",
                AyarValue = "1.5"
            }
        );
        context.SaveChanges();
    }

    // Pazar mesai çarpanı
    if (!context.Lookup_GenelAyarlar.Any(g => g.AyarKey == "PazarMesaiCarpani"))
    {
        context.Lookup_GenelAyarlar.Add(
            new LoyalKullaniciTakip.Data.Lookups.Lookup_GenelAyarlar
            {
                AyarKey = "PazarMesaiCarpani",
                AyarValue = "2.0"
            }
        );
        context.SaveChanges();
    }

    // Günlük çalışma saati
    if (!context.Lookup_GenelAyarlar.Any(g => g.AyarKey == "GunlukCalismaSaati"))
    {
        context.Lookup_GenelAyarlar.Add(
            new LoyalKullaniciTakip.Data.Lookups.Lookup_GenelAyarlar
            {
                AyarKey = "GunlukCalismaSaati",
                AyarValue = "7.5"
            }
        );
        context.SaveChanges();
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Kültür ayarlarını uygula
app.UseRequestLocalization();

app.UseRouting();

//app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
