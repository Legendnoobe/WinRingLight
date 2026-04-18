> ⚠️ **Warning**
> Be careful: This project partly uses AI agents.

# WinRingLight

[English](#english) | [Türkçe](#türkçe)

---

## Screenshots / Ekran Görüntüleri

| ![Menu 1](ScreenShots/Ekran%20görüntüsü%202026-03-28%20012505.png) | ![Menu 2](ScreenShots/Ekran%20görüntüsü%202026-03-28%20012519.png) |
| :---: | :---: |
| ![Menu 3](ScreenShots/Ekran%20görüntüsü%202026-03-28%20012531.png) | ![Menu 4](ScreenShots/Ekran%20görüntüsü%202026-03-28%20012544.png) |

---

## English

WinRingLight is a high-performance, modern, and sleek screen ring light application for Windows, designed to enhance your appearance during video calls and streaming.

### 🌟 Features

- **Modern UI**: Fluent Design inspired interface that feels native to Windows 11.
- **Dynamic Light Modes**: 
    - **Ring**: Classic circular light for natural eyes catchlights.
    - **Frame**: Rounded rectangle overlay.
    - **Sides**: Left and right edge lighting.
    - **Top**: Focused top-edge lighting.
    - **Full**: Full-screen uniform light.
- **Hardware Brightness Control**: Adjusts your monitor's actual backlight level via WMI (Monitor Dimmer).
- **Asymmetric Growth**: Light grows outwards to prevent obscuring your screen content.
- **Smart Interaction**: 
    - **Click-to-Set**: Instantly set slider values by clicking anywhere on the track.
    - **Double-Click to Hide**: Hide the menu instantly by double-clicking any empty screen area.
    - **Global Shortcut**: Use `H` from anywhere (even while gaming) to toggle the light.

### ⌨️ Shortcuts

- **H**: Toggle Menu Show/Hide (System-wide).
- **Double-Click (Empty Space)**: Hide menu when open.
- **Esc**: Exit Application.

### 🚀 Installation

Find the executables in the `publish` folder:
1. **Portable**:    (~170 MB) - Self-contained, runs anywhere.
2. **Standard**: (<1 MB) - Requires .NET 8 Runtime.

---

## Türkçe

WinRingLight, Windows için geliştirilmiş, görüntülü görüşmelerde ve yayınlarda görünümünüzü iyileştirmek için tasarlanmış yüksek performanslı ve modern bir ekran halka ışık uygulamasıdır.

### 🌟 Özellikler

- **Modern Tasarım**: Windows 11 estetiğine (Fluent Design) uygun şık arayüz.
- **Dinamik Işık Modları**:
    - **Halka (Ring)**: Gözlerde doğal ışık yansıması için klasik dairesel ışık.
    - **Çerçeve (Frame)**: Kenarları kavisli dikdörtgen çerçeve.
    - **Yanlar (Sides)**: Ekranın sağ ve sol yanları için ışık.
    - **Üst (Top)**: Ekranın üst kısmına odaklanmış ışık.
    - **Tam (Full)**: Tüm ekranı kaplayan homojen ışık.
- **Donanımsal Parlaklık**: Monitörünüzün gerçek arka ışık (backlight) seviyesini WMI üzerinden ayarlar.
- **Asimetrik Büyüme**: Işık merkeze değil dışa doğru büyür, böylece ekrandaki görüntünüzü kapatmaz.
- **Akıllı Etkileşim**:
    - **Tıkla Ayarla**: Sürgülerin (slider) herhangi bir yerine tıklayarak anında değer atama.
    - **Çift Tıkla Kapat**: Menü açıkken boş ekrana çift tıklayarak menüyü gizleme.
    - **Global Kısayol**: `H` tuşu ile her yerden (oyundayken bile) ışığı açıp kapatabilme.

### ⌨️ Kısayollar

- **H**: Menüyü Göster/Gizle (Tüm sistemde çalışır).
- **Çift Tık (Boş Alan)**: Menü açıkken menü dışına çift tıklamak menüyü gizler.
- **Esc**: Uygulamadan Çık.

### 🚀 Kurulum

`publish` klasörü altındaki sürümlerden birini seçebilirsiniz:
1. **Portable**:  (~170 MB) - Hiçbir kurulum gerektirmez, direkt çalışır.
2. **Standard**:  (<1 MB) - Çalışması için .NET 8 yüklü olmalıdır.

---

## Technical Details / Teknik Detaylar

- **Language**: C# / WPF (.NET 8)
- **APIs**: Win32 Interop (Global Hotkeys, Layered Windows), WMI (Monitor Brightness).
- **Architecture**: Specialized Three-Window hybrid system for perfect click-through and interactivity.
