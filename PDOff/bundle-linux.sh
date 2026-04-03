#!/bin/bash
APP_NAME="PDOff"
PUBLISH_DIR="./bin/Release/net10.0/linux-x64/publish"
OUT="./dist/$APP_NAME-linux"

# Build for Linux if not already published
if [ ! -f "$PUBLISH_DIR/$APP_NAME" ]; then
    echo "Publishing for linux-x64..."
    dotnet publish -c Release -r linux-x64 --self-contained true
fi

# Create output directory
rm -rf "$OUT"
mkdir -p "$OUT"

# Copy all published files
cp -r "$PUBLISH_DIR/." "$OUT/"
chmod +x "$OUT/$APP_NAME"

# Copy icon (PNG preferred on Linux)
if [ -f "./Assets/app.png" ]; then
    cp "./Assets/app.png" "$OUT/app.png"
elif [ -f "./Assets/app.ico" ]; then
    cp "./Assets/app.ico" "$OUT/app.ico"
fi

# Create .desktop file
mkdir -p "$OUT/share/applications"
cat > "$OUT/share/applications/$APP_NAME.desktop" << EOF
[Desktop Entry]
Name=$APP_NAME
Exec=/opt/$APP_NAME/$APP_NAME
Icon=/opt/$APP_NAME/app.png
Type=Application
Categories=Office;Utility;
Comment=PDF utility application
Terminal=false
EOF

# Create install script
cat > "$OUT/install.sh" << 'INSTALL'
#!/bin/bash
APP_NAME="PDOff"
INSTALL_DIR="/opt/$APP_NAME"

if [ "$EUID" -ne 0 ]; then
    echo "Please run as root: sudo bash install.sh"
    exit 1
fi

mkdir -p "$INSTALL_DIR"
cp -r . "$INSTALL_DIR/"
chmod +x "$INSTALL_DIR/$APP_NAME"

# Install desktop entry
cp "share/applications/$APP_NAME.desktop" /usr/share/applications/
update-desktop-database /usr/share/applications/ 2>/dev/null || true

echo "✓ Installed to $INSTALL_DIR"
echo "  You can now launch $APP_NAME from your application menu."
INSTALL
chmod +x "$OUT/install.sh"

echo "✓ Bundle created: $OUT"
echo "  To install system-wide: cd $OUT && sudo bash install.sh"
echo "  To run directly:        $OUT/$APP_NAME"
