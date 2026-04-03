# Microsoft Store Yayınlama Talimatları

Bu klasör, uygulamanızı Microsoft Store'da yayınlamanız için gerekli olan temel yapılandırmayı ve modern ikon setini içerir.

## Hazırlanan Dosyalar
- **`ms-store/Package.appxmanifest`**: Uygulama kimliği, görsel öğeler ve izinlerin (Capabilities) tanımlandığı ana yapılandırma dosyası.
- **`ms-store/Assets/`**: Farklı boyutlarda ve formatlarda optimize edilmiş modern ikonlar ve açılış ekranı (SplashScreen).
- **Yeni İkon Tasarımı**: Uygulamanız için "Ring Light" konseptine uygun, modern ve şık bir ikon seti üretildi.

## Yayınlama Adımları

### 1. Microsoft Partner Center Bilgilerini Güncelleyin
`Package.appxmanifest` dosyasını bir metin düzenleyici ile açın ve şu alanları Partner Center hesabınızdaki bilgilerle güncelleyin:
- `Identity Name`: Partner Center'daki Paket Aile Adı (Package Family Name).
- `Publisher`: Partner Center'daki Yayıncı Kimliği (Publisher ID - örn: `CN=...`).

### 2. Uygulamayı Paketleme
Uygulamanızı MSIX olarak paketlemek için iki ana yolunuz vardır:

#### A Seçeneği: Visual Studio (Önerilen)
1. Çözümünüze (Solution) yeni bir **"Windows Application Packaging Project"** ekleyin.
2. Bu yeni projeye `WinRingLight` ana projenizi referans (Dependency) olarak ekleyin.
3. Projedeki `Package.appxmanifest` dosyasını, buradaki `ms-store/Package.appxmanifest` içeriği ile değiştirin.
4. `Assets` klasöründeki ikonları Visual Studio içinden projenin Assets klasörüne sürükleyip bırakın.
5. Projeye sağ tıklayıp **Publish > Create App Packages** seçeneğini seçin.

#### B Seçeneği: Manuel Paketleme (MSIX Packaging Tool)
- Microsoft Store'dan "MSIX Packaging Tool" uygulamasını indirin ve bu klasördeki manifest ve ikonları kullanarak mevcut `.exe` dosyanızı paketleyin.

## Önemli Notlar
- Uygulama `.NET 8` tabanlı olduğu için, manifest dosyasında `runFullTrust` izni eklenmiştir. Bu, WPF uygulamalarının sistem kaynaklarına erişebilmesi için gereklidir.
- İkonlar 1024x1024 boyutundaki bir ana tasarımdan otomatik olarak ölçeklendirilmiştir.

---
*WinRingLight - Microsoft Store Hazırlık Paketi*
