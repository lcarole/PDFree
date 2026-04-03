#!/bin/bash
set -e
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/../../.." && pwd)"

echo "Publishing PDOff for macOS x64..."
dotnet publish "$ROOT_DIR/PDOff/PDOff.csproj" -c Release -r osx-x64 -o "$ROOT_DIR/publish/macos/x64"
echo "Done: publish/macos/x64"
