#!/bin/bash
APP_NAME="PDOff"
PUBLISH_ARM="./bin/Release/net10.0/osx-arm64/publish"
PUBLISH_X64="./bin/Release/net10.0/osx-x64/publish"
OUT="./dist/$APP_NAME.app"

# Publish for both architectures
echo "Publishing for osx-arm64..."
dotnet publish -c Release -r osx-arm64 --self-contained true -o "$PUBLISH_ARM"

echo "Publishing for osx-x64..."
dotnet publish -c Release -r osx-x64 --self-contained true -o "$PUBLISH_X64"

# Create structure
mkdir -p "$OUT/Contents/MacOS"
mkdir -p "$OUT/Contents/Resources"

# Create universal binary with lipo
echo "Creating universal binary..."
lipo -create "$PUBLISH_ARM/$APP_NAME" "$PUBLISH_X64/$APP_NAME" -output "$OUT/Contents/MacOS/$APP_NAME"
chmod +x "$OUT/Contents/MacOS/$APP_NAME"

# Copy dylibs (universal versions when available, fallback to arm64)
for dylib in "$PUBLISH_ARM/"*.dylib; do
    lib=$(basename "$dylib")
    if [ -f "$PUBLISH_X64/$lib" ]; then
        lipo -create "$dylib" "$PUBLISH_X64/$lib" -output "$OUT/Contents/MacOS/$lib"
    else
        cp "$dylib" "$OUT/Contents/MacOS/$lib"
    fi
done

# Copy icon
cp "./Assets/app.icns" "$OUT/Contents/Resources/app.icns"

# Create Info.plist
cat > "$OUT/Contents/Info.plist" << EOF
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN"
  "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
  <key>CFBundleName</key>
  <string>$APP_NAME</string>
  <key>CFBundleDisplayName</key>
  <string>$APP_NAME</string>
  <key>CFBundleIdentifier</key>
  <string>com.lcarole.pdoff</string>
  <key>CFBundleVersion</key>
  <string>1.0.0</string>
  <key>CFBundleExecutable</key>
  <string>$APP_NAME</string>
  <key>CFBundlePackageType</key>
  <string>APPL</string>
  <key>CFBundleIconFile</key>
  <string>app</string>
  <key>NSHighResolutionCapable</key>
  <true/>
  <key>LSUIElement</key>
  <false/>
</dict>
</plist>
EOF

echo "✓ Bundle created: $OUT"
echo "  Double-click on $APP_NAME.app to launch without terminal."