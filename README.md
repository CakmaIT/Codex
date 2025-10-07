# Özge2

This repository hosts the **Özge2** dual-screen classroom experience for Windows (WPF, .NET 8). The solution is organised into multiple projects to isolate concerns and allow future expansion.

## Solution Layout

```
/Ozge.sln              Solution file referencing all projects
/Ozge.App              WPF presentation layer (Teacher + Projector shells)
/Ozge.Core             Domain models, contracts and messaging primitives
/Ozge.Data             Entity Framework Core, SQLite infrastructure and seeders
/Ozge.Ocr              Offline import + OCR pipeline abstractions
/Ozge.Assets           Static assets (icons, tessdata placeholders, etc.)
/publish               Publish output folder used by build scripts
```

> **Note**: The WPF project targets `net8.0-windows` and therefore must be built on Windows. Linux/macOS environments can edit the code, but the application will only run on Windows.

## Build & Publish

Use the provided PowerShell helper to restore packages, apply migrations and publish a single-file executable under `publish/Ozge2.exe`.

```powershell
./build_publish.ps1
```

The script performs the following steps:

1. `dotnet restore` (solution root)
2. `dotnet build -c Release`
3. `dotnet ef database update` (within `Ozge.Data`)
4. `dotnet publish Ozge.App -c Release -r win-x64 -p:PublishSingleFile=true -p:SelfContained=true -p:IncludeNativeLibrariesForSelfExtract=true -o ./publish`

After publishing, run `publish/Ozge2.exe` on a Windows machine. On the first launch the app initialises `%LOCALAPPDATA%\Ozge2` and prompts for the projector screen.

## Selecting the Projector Screen

1. Launch `Ozge2.exe` on the teacher PC.
2. From the Teacher Dashboard press **Launch Projector**. The projector window opens on the previously selected display (default: primary monitor).
3. To change the display:
   - Move the Projector window to the desired monitor.
   - Close both windows. The selection is persisted for the next launch (future settings integration).

## Development Notes

- The solution wires up a `.NET Generic Host` inside the WPF `App` to share dependency injection, background services and logging across projects.
- The SQLite database (`ozge2.db`) resides under `%LOCALAPPDATA%/Ozge2`. Running the application automatically applies migrations and seeds demo data for four classes.
- The OCR pipeline is represented by `IContentIngestionService` with a deterministic placeholder implementation. Hook in PDF or OCR adapters by extending `Ozge.Ocr`.
- `StateStore` keeps a shared immutable snapshot of class data. The teacher actions broadcast messages to the projector window through `IMessenger` (`CommunityToolkit.Mvvm`).
- UI resources (colours, styles) live under `Ozge.App/Resources` and are merged into the application.

## Prerequisites

- Windows 10/11 with the .NET 8.0 SDK installed
- Visual Studio 2022 or VS Code with C# extensions
- SQLite is bundled via `Microsoft.Data.Sqlite` (no external installation required)

## Status

This commit provides the skeleton for the full Özge2 experience: project structure, DI wiring, EF Core schema and seed data, dual-window shells and messaging infrastructure. Game modes, advanced analytics and OCR features can be implemented incrementally on top of this foundation.
