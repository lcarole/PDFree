$APP_NAME = "PDOff"
$PUBLISH_DIR = ".\bin\Release\net10.0\win-x64\publish"
$OUT = ".\dist\$APP_NAME-windows"

# Build for Windows if not already published
if (-not (Test-Path "$PUBLISH_DIR\$APP_NAME.exe")) {
    Write-Host "Publishing for win-x64..."
    dotnet publish -c Release -r win-x64 --self-contained true
}

# Create output directory
if (Test-Path $OUT) { Remove-Item -Recurse -Force $OUT }
New-Item -ItemType Directory -Path $OUT | Out-Null

# Copy all published files
Copy-Item -Recurse "$PUBLISH_DIR\*" "$OUT\"

# Copy icon
if (Test-Path ".\Assets\app.ico") {
    Copy-Item ".\Assets\app.ico" "$OUT\app.ico"
}

# Create a shortcut script (creates a .lnk on the Desktop)
$SHORTCUT_SCRIPT = "$OUT\create-shortcut.ps1"
@"
`$APP_NAME = "$APP_NAME"
`$TARGET = "`$PSScriptRoot\$APP_NAME.exe"
`$SHORTCUT_PATH = "`$env:USERPROFILE\Desktop\`$APP_NAME.lnk"
`$ICON_PATH = "`$PSScriptRoot\app.ico"

`$WScript = New-Object -ComObject WScript.Shell
`$Shortcut = `$WScript.CreateShortcut(`$SHORTCUT_PATH)
`$Shortcut.TargetPath = `$TARGET
`$Shortcut.IconLocation = `$ICON_PATH
`$Shortcut.Save()

Write-Host "✓ Shortcut created on Desktop"
"@ | Set-Content $SHORTCUT_SCRIPT

Write-Host "✓ Bundle created: $OUT"
Write-Host "  To run directly:              $OUT\$APP_NAME.exe"
Write-Host "  To create a Desktop shortcut: cd $OUT && powershell -ExecutionPolicy Bypass -File create-shortcut.ps1"
