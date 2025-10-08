param(
    [switch]$ApplyMigrations
)

$ErrorActionPreference = 'Stop'

Write-Host 'Restoring packages...'
dotnet restore

Write-Host 'Building Release configuration (managed dependencies)...'
dotnet build Ozge.sln -c Release --no-restore

Write-Host 'Building Ozge.App for win-x64...'
dotnet build Ozge.App/Ozge.App.csproj -c Release -r win-x64 --no-self-contained --no-restore

if ($ApplyMigrations) {
    Write-Host 'Applying database migrations...'
    dotnet ef database update --project Ozge.Data --startup-project Ozge.App
}

Write-Host 'All done.'
