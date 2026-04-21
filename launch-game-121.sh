#!/usr/bin/env bash
set -euo pipefail

VS_APP_DIR="/Applications/Vintage Story 1.21.7.app"
VS_EXECUTABLE="$VS_APP_DIR/Vintagestory"
VS_DATA_PATH="${VS_DATA_PATH:-$HOME/Library/Application Support/VintagestoryData-1.21.7}"

if [[ ! -x "$VS_EXECUTABLE" ]]; then
  echo "Vintage Story 1.21.7 executable not found at: $VS_EXECUTABLE" >&2
  exit 1
fi

echo "Launching Vintage Story 1.21.7 via:"
echo "  $VS_EXECUTABLE"
echo "Using data path:"
echo "  $VS_DATA_PATH"
mkdir -p "$VS_DATA_PATH"
"$VS_EXECUTABLE" --dataPath "$VS_DATA_PATH" >/dev/null 2>&1 &
