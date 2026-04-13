# Build script for minimal cloud gaming setup (no daemon needed)
# Builds: validator, signaling, hub, hid

$env:Path += ';C:\gstreamer\1.22.0\msvc_x86_64\bin'
$env:PKG_CONFIG_PATH = "C:\gstreamer\1.22.0\msvc_x86_64\lib\pkgconfig"

git submodule update --init --recursive

mkdir -Force artifact
mkdir -Force package\validator
mkdir -Force package\signaling
mkdir -Force package\hub\bin
mkdir -Force package\hid

# Download GStreamer libs for hub
Invoke-WebRequest -Uri "https://github.com/thinkonmay/thinkremote-rtchub/releases/download/asset-gstreamer-1.22.0/lib.zip" -OutFile artifact/lib.zip
Expand-Archive artifact/lib.zip -DestinationPath package/hub -Force

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

Write-Host "Build complete! Run script\launch-simple.ps1 to start." -ForegroundColor Green
