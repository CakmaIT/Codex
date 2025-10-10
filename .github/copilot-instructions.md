
# Copilot instructions for the Ozge (Codex) repository

Purpose: Give an AI coding agent the minimal, practical context to make safe, useful edits in this multi-project .NET/WPF solution.

Key points (quick skim)
- Solution: `Ozge.sln` — primary projects: `Ozge.App` (WPF UI, app entry), `Ozge.Core` (shared contracts/models), `Ozge.Data` (EF Core + Sqlite, DbContext & Migrations), `Ozge.Ocr` (Tesseract + PDF parsing), `Ozge.Assets` (static catalogs).
- Runtime composition is in-process; `Ozge.App` references the others and is the startup project.

Patterns & conventions you can rely on
- Namespace == project name (e.g., types in `Ozge.Data` use `Ozge.Data` namespace).
- MVVM: `CommunityToolkit.Mvvm` is used — prefer adding ViewModels and binding over code-behind UI logic.
- Hosting/DI: code uses Microsoft.Extensions.Hosting and Serilog patterns. Look for DI registrations in `Ozge.App` and the `Infrastructure/` folder (e.g., `AppInitializationHostedService.cs`, `BackgroundJobHostService.cs`).
- Data: `OzgeDbContext.cs` lives in `Ozge.Data`. Migrations are under `Ozge.Data/Migrations` and `Microsoft.EntityFrameworkCore.Tools` is available.
- OCR: `Ozge.Ocr` contains `OcrPipeline.cs`. The project targets x64 and depends on native Tesseract binaries and `UglyToad.PdfPig`.

Build / run / debug (exact commands)
- Prereqs: .NET 8 SDK with WPF workload (net8.0-windows) on Windows x64 for OCR/native bits.
- From repo root (PowerShell):
  - Build: dotnet build Ozge.sln -p:Platform=x64
  - Run (fast): dotnet run --project .\Ozge.App\Ozge.App.csproj
  - Run built exe: & .\Ozge.App\bin\Debug\net8.0-windows\Ozge.App.exe

EF Core migrations (how to add/update)
- Ensure `dotnet-ef` is installed: dotnet tool install --global dotnet-ef
- Create migration (run from repo root):
  dotnet ef migrations add <Name> --project .\Ozge.Data\Ozge.Data.csproj --startup-project .\Ozge.App\Ozge.App.csproj
  dotnet ef database update --project .\Ozge.Data\Ozge.Data.csproj --startup-project .\Ozge.App\Ozge.App.csproj

Important local constraints
- `Ozge.App` and `Ozge.Ocr` target x64 (native Tesseract). Do not change runtime/packaging assumptions without a Windows x64 test.
- There are currently no test projects in the repo; prefer small, focused changes and validate by building and running the app.

Where to look when making common edits
- Add UI/ViewModel: `Ozge.App\MainWindow.xaml(.cs)` and `Ozge.App` ViewModel folders (follow existing MVVM usages).
- DI/host changes: `Ozge.App\App.xaml.cs`, `Infrastructure\AppInitializationHostedService.cs`, `Infrastructure\DependencyCheckService.cs`.
- Data changes: `Ozge.Data\OzgeDbContext.cs`, `Ozge.Data\Migrations\`, `Ozge.Data\Services\`.
- OCR work: `Ozge.Ocr\OcrPipeline.cs`, `Ozge.Ocr\Extensions\`.
- Assets/catalogs: `Ozge.Assets\AssetCatalog.cs`.

PR & edit guidance for AI agents
- Make minimal, focused edits. Update only the projects/files required and run `dotnet build` to validate.
- If adding packages, update the appropriate `.csproj` and run a full build for all affected projects (use -p:Platform=x64 when relevant).
- Never modify native Tesseract packaging or platform targets without human review and Windows x64 verification.

Files referenced while writing these instructions: `Ozge.App\Ozge.App.csproj`, `Ozge.App\App.xaml.cs`, `Ozge.App\MainWindow.xaml.cs`, `Ozge.Data\OzgeDbContext.cs`, `Ozge.Data\Migrations\`, `Ozge.Ocr\OcrPipeline.cs`, `Ozge.Assets\AssetCatalog.cs`.

If anything here is unclear or you want additional rules (CI, branching, coding style), tell me which areas to expand.
