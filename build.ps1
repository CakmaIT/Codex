cd F:\Projeler\Github\Codex\Codex
dotnet restore
dotnet build Ozge.sln

dotnet build Ozge.sln -c Release
dotnet build Ozge.App/Ozge.App.csproj -c Release -r win-x64 --no-self-contained --no-restore

Remove-Item -Path F:\Projeler\Github\Codex\Codex\Ozge.App\bin\Release\net8.0-windows\win-x64 -Recurse -Confirm -Force
Remove-Item -Path C:\Users\Yasar\AppData\Local\Ozge2 -Recurse -Confirm -Force

cd F:\Projeler\Github\Codex\Codex
dotnet restore
dotnet build Ozge.sln -c Release
dotnet build Ozge.App/Ozge.App.csproj -c Release -r win-x64 --no-self-contained --no-restore
