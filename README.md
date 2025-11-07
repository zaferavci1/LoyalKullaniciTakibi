# Loyal Kullanıcı Takip Sistemi

Personel takip ve yönetim sistemi - ASP.NET Core Razor Pages ile geliştirilmiştir.

## 📋 Proje Hakkında

Bu proje, personel yönetimi için kapsamlı bir takip sistemidir. Bordro, hakediş, izin, puantaj, zimmet ve muhasebe işlemlerini yönetmek için geliştirilmiştir.

## ✨ Özellikler

- **Personel Yönetimi**: Personel bilgileri, iletişim detayları ve eğitim geçmişi takibi
- **Puantaj Sistemi**: Günlük puantaj kaydı ve takibi
- **İzin Yönetimi**: İzin talepleri, kıdem izin hakediş hesaplamaları
- **Bordro ve Hakediş**: Bordro kayıtları, hakediş hesaplamaları ve fazla mesai yönetimi
- **Muhasebe**: Muhasebe hareketleri ve takibi
- **Zimmet Takibi**: Personele verilen zimmetlerin kaydı ve yönetimi
- **Belge Yönetimi**: Personel belgelerinin yüklenmesi ve saklanması
- **Raporlama**: Çeşitli raporlama özellikleri

## 🛠️ Teknolojiler

- **Framework**: ASP.NET Core 8.0
- **UI**: Razor Pages
- **ORM**: Entity Framework Core
- **Veritabanı**: SQLite
- **Frontend**: Bootstrap 5, jQuery
- **API Documentation**: Swagger/OpenAPI

## 📦 Kurulum

### Gereksinimler

- .NET 8.0 SDK
- Visual Studio 2022 veya VS Code (önerilen)

### Adımlar

1. Projeyi klonlayın:
```bash
git clone https://github.com/kullanici-adiniz/LoyalKullaniciTakip.git
cd LoyalKullaniciTakip
```

2. NuGet paketlerini yükleyin:
```bash
dotnet restore
```

3. Veritabanını oluşturun:
```bash
dotnet ef database update
```

4. Projeyi çalıştırın:
```bash
dotnet run
```

5. Tarayıcınızda `https://localhost:5001` adresini açın.

## 🗄️ Veritabanı Yapısı

Proje aşağıdaki ana tabloları içerir:

- `Personel`: Temel personel bilgileri
- `IletisimBilgileri`: İletişim detayları
- `EgitimBilgileri`: Eğitim geçmişi
- `Belgeler`: Personel belgeleri
- `PuantajGunluk`: Günlük puantaj kayıtları
- `IzinTalepleri`: İzin talep ve onayları
- `BordroHakedisKaydi`: Bordro ve hakediş kayıtları
- `MuhasebeHareketleri`: Muhasebe işlemleri
- `Zimmet`: Zimmet kayıtları

### Lookup Tabloları

- `Lookup_Departmanlar`: Departman bilgileri
- `Lookup_Meslekler`: Meslek tanımları
- `Lookup_CalismaTipi`: Çalışma tipleri
- `Lookup_IzinTipleri`: İzin türleri
- `Lookup_PuantajDurumlari`: Puantaj durumları
- `Lookup_GenelAyarlar`: Sistem ayarları

## 🔧 Yapılandırma

Uygulama ayarlarını `appsettings.json` dosyasından yapılandırabilirsiniz:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=loyalkullanici.db"
  }
}
```

## 📱 Kullanım

### Genel Ayarlar

Sistem genelinde kullanılacak parametreler (asgari ücret, fazla mesai çarpanları vb.) "Ayarlar > Genel Ayarlar" menüsünden yapılandırılabilir.

### Personel İşlemleri

1. **Yeni Personel Ekleme**: Personel menüsünden "Yeni Personel" seçeneği ile
2. **Personel Detayları**: Her personel için SGK, muhasebe ve genel detaylar görüntülenebilir
3. **Belge Yükleme**: Personel belgelerini sisteme yükleyebilirsiniz

### Puantaj ve İzin

- Günlük puantaj girişi yapılabilir
- İzin talepleri oluşturulup takip edilebilir
- Kıdem izin hakedişleri otomatik hesaplanır

## 🚀 Geliştirme

Yeni migration eklemek için:

```bash
dotnet ef migrations add MigrationAdi
dotnet ef database update
```

## 📝 Lisans

Bu proje özel kullanım içindir.

## 👥 Katkıda Bulunma

Proje şu anda aktif geliştirme aşamasındadır. Katkıda bulunmak için lütfen issue açın.

## 📧 İletişim

Sorularınız için issue açabilirsiniz.

---

**Not**: Bu sistem personel verilerini işlediği için KVKK (Kişisel Verilerin Korunması Kanunu) kapsamında gerekli güvenlik önlemlerinin alınması gerekmektedir.

