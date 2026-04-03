#!/bin/bash
set -e
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"

echo "=== Publishing PDOff for all platforms ==="
echo ""

echo "--- Linux arm64 ---"
bash "$SCRIPT_DIR/bundle/Linux/publish-arm64.sh"
echo ""

echo "--- Linux x64 ---"
bash "$SCRIPT_DIR/bundle/Linux/publish-x64.sh"
echo ""

echo "--- macOS arm64 ---"
bash "$SCRIPT_DIR/bundle/MacOS/publish-arm64.sh"
echo ""

echo "--- macOS x64 ---"
bash "$SCRIPT_DIR/bundle/MacOS/publish-x64.sh"
echo ""

echo "--- Windows arm64 ---"
bash "$SCRIPT_DIR/bundle/Windows/publish-arm64.sh"
echo ""

echo "--- Windows x64 ---"
bash "$SCRIPT_DIR/bundle/Windows/publish-x64.sh"
echo ""

echo "=== All builds completed ==="
