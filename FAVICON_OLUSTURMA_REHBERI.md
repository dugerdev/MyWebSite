# Favicon Oluşturma Rehberi - Site Renklerine Uygun

## Site Renk Paleti
- **Primary Mavi:** `#1e30f3`
- **Secondary Pembe:** `#e21e80`
- **Gradient:** 135° açıyla `#1e30f3` → `#e21e80`

## Yöntem 1: Canva ile PNG Oluşturma (Önerilen)

### Adımlar:
1. **Canva'ya gidin:** https://www.canva.com
2. **Yeni tasarım oluşturun:**
   - "Özel boyut" seçin
   - Genişlik: `512 px`
   - Yükseklik: `512 px`
   - Oluştur'a tıklayın

3. **Gradient arka plan ekleyin:**
   - Sol panelden "Öğeler" → "Şekiller" → "Kare" seçin
   - Kareyi sayfaya sürükleyin, tam ekranı kaplayacak şekilde ayarlayın
   - Kareyi seçin → Üstteki renk paleti ikonuna tıklayın
   - "Gradient" seçeneğini seçin
   - Renk 1: `#1e30f3` (mavi)
   - Renk 2: `#e21e80` (pembe)
   - Gradient yönünü 135° olarak ayarlayın (diagonal, sol üstten sağ alta)

4. **Metin ekleyin:**
   - Sol panelden "Metin" → "Başlık Ekle" seçin
   - "MD" veya "DD" yazın
   - Font: **Poppins Bold** veya **Montserrat Bold**
   - Renk: **Beyaz** (#FFFFFF)
   - Boyut: Yaklaşık **300-350px** (tam ortalanması için)
   - Metni sayfanın tam ortasına yerleştirin

5. **PNG olarak indirin:**
   - Sağ üstteki "İndir" butonuna tıklayın
   - Dosya türü: **PNG**
   - Kalite: **Yüksek**
   - İndir'e tıklayın

## Yöntem 2: Online Gradient Generator + Text Overlay

1. **Gradient oluşturun:**
   - https://cssgradient.io/ sitesine gidin
   - Gradient ayarları:
     - Color 1: `#1e30f3` (pozisyon: 0%)
     - Color 2: `#e21e80` (pozisyon: 100%)
     - Açı: `135deg`
   - 512x512 px boyutunda gradient'i PNG olarak indirin

2. **Text eklemek için:**
   - https://www.photopea.com/ (ücretsiz Photoshop alternatifi) veya Canva kullanın
   - Gradient PNG'yi açın
   - "MD" veya "DD" metni ekleyin (beyaz, bold font)
   - PNG olarak export edin

## Yöntem 3: HTML'den PNG'e Çevirme

1. Projede oluşturduğumuz `favicon-generator.html` sayfasını açın
2. Tarayıcıda görüntüleyin
3. Ekran görüntüsü alın (512x512 px alanı)
4. Image editor ile kırpın

## Yöntem 4: Figma Kullanma (Ücretsiz)

1. https://www.figma.com/ sitesine gidin (ücretsiz hesap)
2. 512x512 px frame oluşturun
3. Rectangle ekleyin ve gradient uygulayın:
   - Fill: Linear Gradient
   - Color 1: `#1e30f3`
   - Color 2: `#e21e80`
   - Angle: 135°
4. "MD" metni ekleyin (beyaz, bold)
5. Export → PNG → 1x → Export

## RealFaviconGenerator ile Dönüştürme

512x512 PNG hazır olduktan sonra:

1. https://realfavicongenerator.net/ sitesine gidin
2. "Select your Favicon image" butonuna tıklayın
3. Oluşturduğunuz 512x512 PNG dosyasını seçin
4. Ayarları kontrol edin (genellikle default ayarlar yeterlidir)
5. "Generate your Favicons and HTML code" butonuna tıklayın
6. "Favicon package" butonuna tıklayarak tüm dosyaları indirin
7. İndirilen dosyaları `wwwroot/assets/` klasörüne kopyalayın

## Son Adım: Dosyaları Güncelleme

1. Eski favicon dosyalarını silin:
   - `wwwroot/assets/favicon.ico`
   - `wwwroot/assets/apple-touch-icon.png`
   - `wwwroot/assets/favicon-96x96.png`
   - `wwwroot/assets/web-app-manifest-192x192.png`
   - `wwwroot/assets/web-app-manifest-512x512.png`
   - `wwwroot/assets/site.webmanifest`

2. Yeni indirdiğiniz favicon paketindeki dosyaları kopyalayın:
   - `favicon.ico`
   - `apple-touch-icon.png`
   - `favicon-96x96.png`
   - `web-app-manifest-192x192.png`
   - `web-app-manifest-512x512.png`
   - `site.webmanifest`

3. Projeyi yeniden başlatın ve tarayıcı cache'ini temizleyin (`Ctrl + Shift + R`)

