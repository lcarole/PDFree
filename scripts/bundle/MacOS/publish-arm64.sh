#!/bin/bash
set -e
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/../../.." && pwd)"

echo "Publishing PDOff for macOS arm64..."
dotnet publish "$ROOT_DIR/PDOff/PDOff.csproj" -c Release -r osx-arm64 -o "$ROOT_DIR/publish/macos/arm64"
echo "Done: publish/macos/arm64"
