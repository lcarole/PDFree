#!/bin/bash
set -e
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/../../.." && pwd)"

echo "Publishing PDOff for Linux arm64..."
dotnet publish "$ROOT_DIR/PDOff/PDOff.csproj" -c Release -r linux-arm64 -o "$ROOT_DIR/publish/linux/arm64"
echo "Done: publish/linux/arm64"
