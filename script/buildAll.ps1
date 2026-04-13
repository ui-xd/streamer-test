# Build script for cloud gaming setup
# Prerequisites: GStreamer 1.22.0, Go 1.20+, .NET 7, Node.js

$env:Path += ';C:\gstreamer\1.22.0\msvc_x86_64\bin'
$env:PKG_CONFIG_PATH = "C:\gstreamer\1.22.0\msvc_x86_64\lib\pkgconfig"

git submodule update --init --recursive

mkdir -Force package\validator | Out-Null
mkdir -Force package\signaling | Out-Null
mkdir -Force package\hub\bin | Out-Null
mkdir -Force package\hid | Out-Null

go clean --cache

# Build validator service
Write-Host "Building validator service..." -ForegroundColor Cyan
Set-Location .\validator-service
go build -o validator-service.exe
Set-Location ..
robocopy .\validator-service package\validator validator-service.exe

# Build signaling server
Write-Host "Building signaling server..." -ForegroundColor Cyan
Set-Location .\signaling
go build -o signaling.exe ./cmd/
Set-Location ..
robocopy .\signaling package\signaling signaling.exe

# Build hub
Write-Host "Building hub..." -ForegroundColor Cyan
Set-Location .\server\hub
go build -o hub.exe ./cmd/server/
Set-Location ..\..
robocopy .\server\hub package\hub\bin hub.exe
Remove-Item ".\server\hub\hub.exe" -ErrorAction SilentlyContinue

# Build HID
Write-Host "Building HID server..." -ForegroundColor Cyan
Set-Location .\server\hid
dotnet build . --output "bin" --self-contained true --runtime win-x64
Set-Location ..\..
robocopy .\server\hid\bin package\hid

Write-Host "Build complete! Run .\script\launch.ps1 to start." -ForegroundColor Green
