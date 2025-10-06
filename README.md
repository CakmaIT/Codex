# Neşeli İngilizce Sınıf Uygulaması

Bu depo, 5. sınıf öğrencileri için hazırlanan projeksiyon dostu "Neşeli İngilizce" etkinlik sayfasını içerir. Uygulama; öğrencilerin adlarını ekleyip rastgele seçebildikleri, tek menüden yönetilen renkli mini oyunlar (kelime çarkı, hız testi, sınıf görevleri, kelime avı ve emoji hikâyesi) ile puan panosunu destekler.

## Dosya Yapısı

- `index.html` – Sayfanın iskeletini ve etkinlik bölümlerini barındırır.
- `style.css` – Renkli arayüz, tipografi ve düzen stillerini tanımlar.
- `script.js` – Öğrenci kayıtları, puan panosu, oyun menüsü geçişleri ve tüm mini oyunların işlevselliğini yönetir.

## Nasıl Çalıştırılır?

1. Depoyu yerel makinenize klonlayın veya ZIP olarak indirin.
2. Dosyaları açın ve `index.html` dosyasını çift tıklayarak tarayıcınızda açın.
   - Alternatif olarak, statik bir sunucu kullanmak isterseniz, depo klasöründe şu komutu çalıştırabilirsiniz:
     ```bash
     npx serve .
     ```
     Ardından tarayıcınızdan belirtilen yerel adrese gidin (örneğin `http://localhost:3000`).
3. Projeksiyon cihazınızı bağlayarak tarayıcı penceresini tam ekrana alın. Uygulama interaktif olduğundan, öğretmen bilgisayarı veya akıllı tahta üzerinden kontrol edilebilir.

## İçerik Güncelleme İpuçları

- Yeni kelimeler eklemek için `script.js` dosyasındaki `vocabulary` dizisine yeni nesneler ekleyin.
- Kelime avı oyununun havuzuna yeni sözcükler eklemek için yine `vocabulary` dizisini kullanın; rastgele seçilip karıştırılır.
- Sınıf meydan okumalarını `challenges` dizisinde düzenleyin veya yenilerini ekleyin.
- Emoji hikâyeleri ve yönlendirme cümlelerini `emojiStories` dizisine ekleyerek çeşitlendirebilirsiniz.
- Öğrenci listesi sınıfta girildiği için varsayılan isim bulunmaz; projeksiyon öncesi öğrencileri eklemeyi unutmayın.

## Test / Kontrol

- Uygulama statik HTML, CSS ve JavaScript dosyalarından oluştuğu için ekstra bir derleme adımı gerekmez.
- Tarayıcı üzerinde çalıştırırken konsol hata kayıtlarını (DevTools > Console) kontrol etmek, olası sorunları tespit etmenize yardımcı olur.

İyi dersler! 🎉
