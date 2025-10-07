param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64"
)

Write-Host "Restoring packages..."
dotnet restore Ozge.sln

Write-Host "Building..."
dotnet build Ozge.sln -c $Configuration

Write-Host "Applying database migrations..."
dotnet ef database update --project Ozge.Data --startup-project Ozge.App --context Ozge.Data.Context.OzgeDbContext -c $Configuration

Write-Host "Publishing single-file package..."
dotnet publish Ozge.App -c $Configuration -r $Runtime --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish

Write-Host "Publish complete. Output located at ./publish/Ozge2.exe if rename is required."
