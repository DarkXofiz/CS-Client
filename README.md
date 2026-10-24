# CsClient — Android overlay + Unity gorsel/debug menusu

Android overlay (`android-app`) ve kendi Unity projen icin gorsel/debug menusu (`unity-plugin`). Aimbot, nisan otomasyonu, hafiza okuma veya ESP icermez.

## Android APK (GitHub Actions)
1. Depoya bu klasorun icerigini yukle.
2. **Actions** -> **Build CsClient Android** -> **Run workflow**.
3. `CsClient-debug-apk` artifact'ini indir, kur, "Diger uygulamalarin ustunde goster" iznini ver.

Yerel derleme: JDK 17 + Android SDK 35 + Gradle 8.9+, `android-app` icinde `gradle assembleDebug`.

## Unity kurulumu
Android overlay ile Unity AYNI APK'da olmali (paket adi `com.wetsuki.csclient`), cunku broadcast sadece uygulamanin icinde calisir.
1. `unity-plugin/Assets` icerigini kendi Unity projenin `Assets` klasorune kopyala.
2. Sahnede "AndroidCommandBridge" adli bir GameObject olustur; ona `AndroidCommandBridge`, `CsClientAndroidInstaller` ve `WetsukiMenu` bilesenlerini ekle.
3. Unity'de `Player` etiketini olustur ve test nesnelerine ver (hedef isaretleri icin).
4. Overlay kodunu (`OverlayService`, `MainActivity`, `KeyVpnService`) ve manifest girdilerini ayni Android projesine dahil et.

Menu ayarlari: FOV cemberi, cember boyutu, kalinlik, renk, hedef isaretleri, mesafe etiketleri, merkez cizgileri. Komutlar `isim:deger` biciminde gelir (`fov:120`, `color:2`, `FOV circle:off`). Klavyeli cihazda F6 menuyu gizler/gosterir.

## V11.4
- `TargetLock.cs` eklendi (kendi Unity oyunun icin hedef kilidi / lock-on): kamerayi en yakin gorunen hedefe yumusakca cevirir. Hedefler `Player` etiketli ya da `TestTarget` bilesenli nesnelerdir. Sag alttaki "Kilit" tusuyla acilir. Duvar arkasindaki hedefe kilitlenmez.

## V11.3
- `StraightBullet.cs` eklendi (kendi Unity oyunun icin): mermi isinla ilerler, fizik kullanmadigi icin sekmez. Mermi prefabindan Rigidbody ve Collider'i kaldir.

## V11.2 (Unity eklentisi)
- Hedef isaretleri artik "Player" etiketi tanimli olmasa da calisir: nesneye `TestTarget` bileseni eklemek yeterli. Sahneye `TestTargetSpawner` eklersen oyun baslarken kameranin onune 5 test kupu olusturur.
- Kamera `MainCamera` etiketli degilse ilk kamera kullanilir. Menude durum satiri nedenini yazar (kamera yok / hedef yok / etiket yok).
- Isaretler dolu kutu yerine kontur kutu, "Merkez cizgileri" artik hedef olmasa da ekran ortasina cizilir.
- Menu yuksek DPI telefonlarda ekrana gore buyutulur.
- Not: bunlar Unity projesinde calisir; android-app APK'si tek basina sadece daire/cizgi cizer.

## V11.1
- Daire artik tam ekran (MATCH_PARENT) pencere yerine ekran ortasinda sabit piksel boyutlu kucuk pencerelerle cizilir; bazi cihazlarda tam ekran overlay hic gorunmuyordu. Merkez cizgileri de ayri ince pencereler.

## V11 (V10 duzeltmeleri)
- **Asil sorun:** V10 zip'inin icinde ekstra bir `CsClient/` klasoru vardi. Push edilince `.github/workflows` deponun ana dizininde degil `CsClient/` icinde kaliyor, GitHub Actions hic calismiyor ve APK uretilmiyordu. V11'de dosyalar zip'in kok dizininde.
- **Overlay artik kendi basina calisiyor:** Cember ve merkez cizgileri artik `OverlayService` tarafindan ekranin ortasina cizilir (dokunmayi alta gecirir). Eskiden menu sadece Unity'ye broadcast atiyordu; Unity ayni APK'da olmadigi icin hicbir dugme bir sey yapmiyordu.
- Menu yatay (landscape) ekranda tasmiyor: kaydirilabilir ve ekrana sigacak sekilde konumlaniyor.
- Izin akisi duzeltildi: "ustte gosterme" izni verilmezse sonsuz donguye girmiyor; VPN izni reddedilirse menu yine aciliyor.
- Overlay penceresi eklenemezse (izin yok vb.) uygulama cokmek yerine mesaj gosterip duruyor.
- Cutout/notch'lu ekranlarda cember gercek ekran merkezinde.
- `buildToolsVersion 35.0.0` sabitlendi (workflow'un kurdugu surumle ayni), workflow'da APK bulunamazsa hata veriyor, `.gitignore` eklendi.
- Unity tarafi: tek `WetsukiMenu`, Android komut baglantisi, 0.5 sn aralikli tarama, F6 sadece eski Input Manager'da. "Target markers" ve "Distance labels" sadece Unity sahnesindeki `Player` etiketli kendi test nesnelerini isaretler.


## V11.5
- ESP/debug çizimleri kendi Unity sahnesindeki `Player` etiketli veya `TestTarget` bileşenli nesneler için düzeltildi.
- Renderer sınırlarından kontur kutusu ve baş/gövde/ayak referans noktaları çizilir.
- Mesafe etiketleri, merkez çizgileri ve renk/çember ayarları korunur.
- Rakibe otomatik nişan alma veya atışları rakibe yönlendirme eklenmedi; bu paket üçüncü taraf oyunlarda hile için değildir.


## V11.7
ESP/FOV visual debug code updated. See `README-V11.7.md` for Unity setup and limitations.


## V11.9
Derleme hatasi (`TestTarget.All`), doku sizintisi, bozuk simgeler ve telefonda menunun geri acilamamasi duzeltildi. Ayrintilar: `README-V11.9.md`.
