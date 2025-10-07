# Özge2 Classroom Suite

This repository contains a .NET 8 WPF solution scaffold for the **Özge2** dual-screen classroom application. The codebase is organised into layered projects following an onion/hexagonal architecture and targets offline-first deployments with SQLite persistence.

## Solution Layout

```
/Ozge.sln                Solution file referencing all projects
/Ozge.App               WPF presentation layer (Teacher + Projector windows)
/Ozge.Core              Domain entities, contracts and helpers
/Ozge.Data              Entity Framework Core persistence + seeding
/Ozge.Ocr               Offline content parsing pipeline abstractions
/Ozge.Assets            Resource container for tessdata/images/sounds
/publish                Output directory for single-file builds
```

## Getting Started

1. Install the .NET 8 SDK and the EF Core CLI tools.
2. (Optional) Populate `Ozge.Assets/tessdata` with Tesseract language packs (e.g. `eng.traineddata`).
3. Run the helper script to restore, migrate and publish:
   ```powershell
   ./build_publish.ps1
   ```
4. Launch the generated `publish/Ozge.App.exe` (rename to `Ozge2.exe` if desired). On first run, the app seeds demo data for classes 5A–5D and opens the Teacher Dashboard window. The Projector window is launched on the configured display; use the “Cycle Projector Screen” button to select the correct monitor and the setting is stored in the local SQLite database at `%LOCALAPPDATA%\Ozge2\ozge2.db`.

## Dual-Screen Operation

- **TeacherDashboardWindow** (primary monitor): provides class selection, group scores and control toggles. Only the Teacher view shows correct answers and moderation controls.
- **ProjectorWindow** (secondary monitor): full-screen, student-facing display. The sample implementation displays the active mode name and can be expanded to support full gameplay experiences.

## Data & Persistence

- SQLite database lives under `%LOCALAPPDATA%/Ozge2/ozge2.db` with Write-Ahead Logging and tuned PRAGMA settings for resilience.
- `DatabaseInitializer` automatically migrates schema and seeds demo classes, groups, students, units, words and quiz questions for rapid evaluation.
- `ImmutableAppStateStore` provides an in-memory, observable snapshot of key entities shared between the Teacher and Projector windows.

## Content Pipeline

`Ozge.Ocr` ships with an `OfflineContentParser` that demonstrates PDF ingestion using UglyToad.PdfPig. The parser reads local PDF files and extracts a simple list of candidate words, producing a single unit placeholder. Extend this component to plug in OCR (Tesseract) and richer clustering heuristics.

## Build Notes

- Projects target `net8.0` / `net8.0-windows` and already reference key packages (CommunityToolkit.Mvvm, EF Core, PdfPig, Tesseract, LiveCharts, Serilog).
- Publishing uses single-file, self-contained options as required. Update the `build_publish.ps1` script to include additional assets or trimming preferences if necessary.
- Asset folders (`Ozge.Assets/images`, `Ozge.Assets/sounds`, `Ozge.Assets/tessdata`) are placeholders—place runtime resources there and ensure the publish profile copies them to `%LOCALAPPDATA%/Ozge2` on first launch.

## Next Steps

The current implementation focuses on scaffolding, data models, and dual-window bootstrapping. Implement mode-specific gameplay, analytics, speech processing, drawing canvas logic, backup rotation, content manager workflows, and the remaining requirements iteratively while keeping the offline, resilient architecture intact.
