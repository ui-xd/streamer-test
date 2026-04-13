# Launch script for cloud gaming
# Run after buildAll.ps1

param(
    [string]$GStreamerPath = "C:\gstreamer\1.22.0\msvc_x86_64",
    [string]$PackagePath = ".\package"
)

$env:Path += ";$GStreamerPath\bin"
$env:PKG_CONFIG_PATH = "$GStreamerPath\lib\pkgconfig"

# Base64-encoded configs (RawURL encoding, no padding)
# gRPC: {"wsurl":"ws://localhost:8088/api/handshake","grpcport":8000,"grpcip":"localhost"}
$grpcConfig = "eyJ3c3VybCI6IndzOi8vbG9jYWxob3N0OjgwODgvYXBpL2hhbmRzaGFrZSIsImdycGNwb3J0Ijo4MDAwLCJncnBjaXAiOiJsb2NhbGhvc3QifQ"

# WebRTC: {"iceServers":[{"urls":["stun:stun.l.google.com:19302"]}]}
$webrtcConfig = "eyJpY2VTZXJ2ZXJzIjpbeyJ1cmxzIjpbInN0dW46c3R1bi5sLmdvb2dsZS5jb206MTkzMDIiXX1dfQ"

Write-Host "Starting cloud gaming stack..." -ForegroundColor Green

# 1. Validator service
Write-Host "Starting validator service on :9090..." -ForegroundColor Cyan
$validator = Start-Process -FilePath "$PackagePath\validator\validator-service.exe" `
    -PassThru -NoNewWindow

# 2. Signaling server
Write-Host "Starting signaling server (WebSocket :8088, gRPC :8000)..." -ForegroundColor Cyan
$signaling = Start-Process -FilePath "$PackagePath\signaling\signaling.exe" `
    -ArgumentList "--websocket", "8088", "--grpc", "8000", "--validationurl", "http://localhost:9090/validate" `
    -PassThru -NoNewWindow

Start-Sleep -Seconds 1

# 3. HID server
Write-Host "Starting HID input server on :5000..." -ForegroundColor Cyan
$hid = Start-Process -FilePath "$PackagePath\hid\hid.exe" `
    -ArgumentList "--urls", "http://localhost:5000" `
    -PassThru -NoNewWindow

Start-Sleep -Seconds 1

# 4. Hub
Write-Host "Starting Hub (WebRTC streaming)..." -ForegroundColor Cyan
$hub = Start-Process -FilePath "$PackagePath\hub\bin\hub.exe" `
    -ArgumentList "--token", "server", "--hid", "localhost:5000", "--grpc", $grpcConfig, "--webrtc", $webrtcConfig `
    -PassThru -NoNewWindow

Write-Host ""
Write-Host "All services started!" -ForegroundColor Green
Write-Host ""
Write-Host "Open in browser:" -ForegroundColor Yellow
Write-Host "  http://<VM_IP>:3000/?signaling=<BASE64_SIGNALING_URL>&token=client" -ForegroundColor White
Write-Host ""
Write-Host "Generate signaling param for your VM IP:" -ForegroundColor Yellow
Write-Host '  [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes("ws://<VM_IP>:8088/api/handshake")).TrimEnd("=")' -ForegroundColor White
Write-Host ""
Write-Host "Press Ctrl+C to stop all services" -ForegroundColor Yellow

try {
    Wait-Process -Id $hub.Id
} finally {
    Write-Host "Shutting down services..." -ForegroundColor Red
    $hub, $hid, $signaling, $validator | ForEach-Object {
        if ($_ -and !$_.HasExited) { Stop-Process -Id $_.Id -Force }
    }
}
