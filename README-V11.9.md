# CsClient V11.9 - hata duzeltmeleri

- **Derleme hatasi:** `TargetLock.cs` `TestTarget.All` kullaniyordu ama `TestTarget` icinde yoktu; Unity'de derleme hatasi veriyordu. `TestTarget` artik aktif hedefleri `All` listesinde tutuyor (`WetsukiMenu` da `FindObjectsOfType` yerine bunu kullaniyor).
- **Bellek sizintisi:** Renk kutusu icin her OnGUI olayinda yeni Texture2D olusturuluyordu. Artik `GUI.color` ile beyaz doku kullaniliyor; menu kapaninca dokular yok ediliyor.
- **Bozuk simgeler:** Unity'nin varsayilan fontunda olmayan karakterler (`◉ ◎ ◈ ⚙ •`) kutu olarak gorunuyordu; duz yazi/ASCII ile degistirildi.
- **Menu geri acilamiyordu:** Telefonda F6 olmadigi icin "Hide menu" sonrasi menu bir daha acilamiyordu. Menu gizliyken sol ustte kucuk bir "Menu" tusu cikiyor.
- **Surum yazilari:** APK ve menu basliklari hala `V11.1` gosteriyordu; `11.9` (versionCode 119) yapildi.
