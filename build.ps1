cd F:\Projeler\Github\Codex\Codex
dotnet restore
dotnet build Ozge.sln

dotnet build Ozge.sln -c Release
dotnet build Ozge.App/Ozge.App.csproj -c Release -r win-x64 --no-self-contained --no-restore


cd F:\Projeler\Github\Codex\Codex
dotnet restore
dotnet build Ozge.sln -c Release
dotnet build Ozge.App/Ozge.App.csproj -c Release -r win-x64 --no-self-contained --no-restore
