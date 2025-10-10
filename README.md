# Ozge2

Offline WPF classroom orchestration suite targeting dual-screen setups. The solution follows a layered architecture with EF Core + SQLite persistence, MVVM presentation, and background services for OCR and content ingestion.

## Prerequisites
- [Microsoft .NET 8 Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) (x64)
- Windows 10/11 with two displays configured (for projector separation)
- Optional: `dotnet-ef` CLI (`dotnet tool install --global dotnet-ef`)

## Repository Layout
```
Ozge.sln
├─ Ozge.App      # WPF presentation layer (Teacher dashboard + Projector surface)
├─ Ozge.Core     # Domain entities, state models, contracts
├─ Ozge.Data     # EF Core DbContext, migrations, seeding helpers
├─ Ozge.Ocr      # PDF/OCR ingestion pipeline (UglyToad.PdfPig + Tesseract)
├─ Ozge.Assets   # Embedded resource pack (tessdata placeholder etc.)
├─ build_release.ps1
└─ README.md
```

## First Build
```powershell
./build_release.ps1
```
Script adımları `dotnet restore` ve `dotnet build -c Release`. RID için ayrı olarak `dotnet build Ozge.App/Ozge.App.csproj -c Release -r win-x64 --no-self-contained --no-restore` çalışır. Migrasyon uygulamak istersen:
```powershell
./build_release.ps1 -ApplyMigrations
```

Çıktı klasörü: `Ozge.App\bin\Release\net8.0-windows\win-x64\` (giriş noktası `Ozge2.exe`).

## Runtime Behaviour
1. **Startup sequence**
   - `%LOCALAPPDATA%\Ozge2` altında `Snapshots`, `Backups`, `Logs`, `tessdata` klasörlerini oluşturur.
   - Eksikse `eng.traineddata` dahil gömülü tessdata varlıklarını kopyalar.
   - EF Core otomatik migrasyon çalıştırır, seed verileri yükler ve paylaşılan durumu hydrates.
2. **Dual window orchestration**
   - Uygulama yalnızca öğretmen panelini açar; **Open Projector** butonuna basılana kadar öğrenci ekranı gösterilmez.
   - Projektör açıldığında ilk tespit edilen ikincil monitöre tam ekran taşınır; freeze/reveal butonları yalnızca pencere açıksa etkili olur.
3. **State synchronisation**
   - `AppStateStore` immutable durumları Teacher/Projector view-model’lerine iletir.
   - Reveal / Freeze ve sınıf seçimi değişiklikleri anında senkronize edilir.

## Selecting the Projector Display
1. Windows ekran ayarlarından projektörü etkinleştir.
2. Varsayılan olarak ilk ikincil monitör seçilir. Başka ekran kullanmak için Windows’taki sıralamayı değiştirip uygulamayı yeniden başlat.
3. Gelecekte kalıcı seçim için `IDisplayService` gövdesi hazır (UI henüz yok).

## Database & Assets
- Veritabanı: `%LOCALAPPDATA%\Ozge2\ozge2.db`
- Yedekler: `%LOCALAPPDATA%\Ozge2\Backups`
- Çizim kayıtları: `%LOCALAPPDATA%\Ozge2\Snapshots`
- Loglar: `%LOCALAPPDATA%\Ozge2\Logs`
- OCR verisi: `%LOCALAPPDATA%\Ozge2\tessdata\eng.traineddata`

Migrasyonları manuel çalıştırmak için:
```powershell
dotnet ef database update --project Ozge.Data --startup-project Ozge.App
```

## Dependency Checks
Başlangıçta aşağıdaki bağımlılıklar doğrulanır; eksikse uygulama hata mesajı gösterir:
- `e_sqlite3.dll` (SQLite native driver)
- `%LOCALAPPDATA%\Ozge2\tessdata\eng.traineddata`
- SkiaSharp native kitaplıkları (gerekirse Visual C++ 2015-2022 Redistributable yüklenmeli)

## Feature Highlights
- MVVM tabanlı öğretmen paneli, sınıf seçici ve projektör kontrolü
- Tam ekran Projektör penceresi, freeze/reveal overlay’leri
- Immutable uygulama durumu ve startup hosted service (EF seeding + asset provisioning)
- EF Core model seti: Class, Student, Group, Unit, Word, Question, Session, ScoreEvent, BehaviorEvent, Snapshot, Setting, Attendance, LessonLog, Job
- OCR pipeline: PDF text layer + Tesseract fallback
- Serilog dosya loglama (günlük döngüsü, 7 gün saklama)

## Known Next Steps
- Tam oyun modları (Quiz, Puzzle, Speak vb.) için UI/logic
- Content Manager heuristiklerini genişletme, tessdata’yı gerçek dosya ile değiştirme
- Projektör ekran seçimini UI üzerinden kaydetme
- Analytics panelleri ve PDF/CSV export
- Gerçek iş kuyruğu & background worker implementasyonu

Mutlu dersler!
