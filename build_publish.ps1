param(
    [string]$Configuration = "Release"
)

Write-Host "==> Restoring packages" -ForegroundColor Cyan
& dotnet restore Ozge.sln
if ($LASTEXITCODE -ne 0) { throw "Restore failed" }

Write-Host "==> Building ($Configuration)" -ForegroundColor Cyan
& dotnet build Ozge.sln -c $Configuration
if ($LASTEXITCODE -ne 0) { throw "Build failed" }

Write-Host "==> Applying database migrations" -ForegroundColor Cyan
Push-Location Ozge.Data
& dotnet ef database update
if ($LASTEXITCODE -ne 0) { Pop-Location; throw "Migration failed" }
Pop-Location

Write-Host "==> Publishing single-file executable" -ForegroundColor Cyan
& dotnet publish Ozge.App -c $Configuration -r win-x64 -p:PublishSingleFile=true -p:SelfContained=true -p:IncludeNativeLibrariesForSelfExtract=true -o ./publish
if ($LASTEXITCODE -ne 0) { throw "Publish failed" }

Write-Host "Publish complete -> $(Join-Path (Get-Location) 'publish/Ozge2.exe')" -ForegroundColor Green
