$release = "https://github.com/icanki/spm/files/545961/spm.zip"
$localAppData = $env:LOCALAPPDATA

Write-Host "Creating directory..."
$appFolder = [System.IO.Directory]::CreateDirectory([System.IO.Path]::Combine($localAppData,"SPM"))

Get-ChildItem $appFolder.FullName | Remove-Item -Recurse -Force

Write-Host "Downloading bootstrapper..." -NoNewline

$wc= New-Object System.Net.WebClient
$zip = [System.IO.Path]::Combine($appFolder.FullName, "spm.zip")
$wc.DownloadFile($release, $zip)

Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::ExtractToDirectory($zip, $appFolder.FullName)

Write-Host "Done" -ForegroundColor Green

Write-Host "Download app..." -NoNewline
$zip = [System.IO.Path]::Combine($appFolder.FullName, "app.zip")
$wc.DownloadFile("http://spmservices.azurewebsites.net/api/files/getLastVersion", $zip)
$appShellFolder = [System.IO.Directory]::CreateDirectory([System.IO.Path]::Combine($appFolder.FullName,"App"))
[System.IO.Compression.ZipFile]::ExtractToDirectory($zip, $appShellFolder.FullName)
Write-Host "Done" -ForegroundColor Green

Write-Host "Write Path..." -NoNewline
$path = [Environment]::GetEnvironmentVariable("Path", "User")
if (-not $path.Contains($appFolder.FullName)) {
[Environment]::SetEnvironmentVariable("Path", $path+';'+$appFolder.FullName, "User")
}
Write-Host "Done" -ForegroundColor Green