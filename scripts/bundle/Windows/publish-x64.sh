#!/bin/bash
set -e
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/../../.." && pwd)"

echo "Publishing PDOff for Windows x64..."
dotnet publish "$ROOT_DIR/PDOff/PDOff.csproj" -c Release -r win-x64 -o "$ROOT_DIR/publish/windows/x64"
echo "Done: publish/windows/x64"
