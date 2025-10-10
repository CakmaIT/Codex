param(
  [string]$ProjectPath = "F:\Projeler\Github\Codex\Codex",
  [string]$Owner       = "CakmaIT",
  [string]$Repo        = "Codex",
  [string]$Branch      = "Ozgev4",
  [string]$GitToken    = "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"
)

$ErrorActionPreference = "Continue"

if (-not (Test-Path $ProjectPath)) { throw "Klasör yok: $ProjectPath" }
Set-Location $ProjectPath

# Repo & kullanıcı
if (-not (Test-Path ".git")) { git.exe init | Out-Null }
if (-not (git.exe config user.name 2>$null))  { git.exe config user.name  "$Owner"  | Out-Null }
if (-not (git.exe config user.email 2>$null)) { git.exe config user.email "$Owner@users.noreply.github.com" | Out-Null }
git.exe config credential.helper "" | Out-Null
git.exe config --unset-all http.extraheader 2>$null | Out-Null

# .gitignore (yoksa oluştur)
if (-not (Test-Path ".gitignore")) {
@"
# Build outputs
bin/
obj/
[Bb]uild/
[Ll]ogs/
[Dd]ebug*/
[Rr]elease*/
x64/
x86/
[Aa]rtifacts/
*.log
*.tmp
*.cache
*.db-wal
*.db-journal

# VS / IDE
.vs/
*.user
*.suo
*.ide

# NuGet / Test
packages/
TestResults/
*.nupkg
*.snupkg

# Publish
publish/
*.Publish.xml

# WPF generated
*.g.cs
*.g.i.cs
*.baml
"@ | Set-Content -Path ".gitignore" -Encoding UTF8
}

# Branch’e geç/oluştur
git.exe checkout -B $Branch 2>$null | Out-Null

# Build çıktıları index’ten temizle, sonra yeniden ekle
git.exe rm -r --cached . 2>$null | Out-Null
git.exe add -A

# Commit (boş commit serbest)
git.exe commit --allow-empty -m ("clean src commit {0}" -f (Get-Date -Format 'yyyy-MM-dd HH:mm:ss')) 2>$null | Out-Null

# Token’lı origin ve push
$enc = [System.Uri]::EscapeDataString($GitToken)
$remoteUrl = ("https://{0}:{1}@github.com/{0}/{2}.git" -f $Owner,$enc,$Repo)
if (git.exe remote get-url origin 2>$null) { git.exe remote set-url origin $remoteUrl } else { git.exe remote add origin $remoteUrl }

git.exe push -u origin $Branch --verbose
