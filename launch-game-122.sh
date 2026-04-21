#!/usr/bin/env bash
set -euo pipefail

VS_APP_DIR="/Applications/Vintage Story 1.22.app"
VS_LAUNCHER="${VS_LAUNCHER:-$HOME/bin/vs-1.22}"
VS_EXECUTABLE="$VS_APP_DIR/Vintagestory"

if [[ -x "$VS_LAUNCHER" ]]; then
  echo "Launching Vintage Story 1.22 via:"
  echo "  $VS_LAUNCHER"
  "$VS_LAUNCHER" >/dev/null 2>&1 &
elif [[ -x "$VS_EXECUTABLE" ]]; then
  echo "Launching Vintage Story 1.22 via:"
  echo "  $VS_EXECUTABLE"
  "$VS_EXECUTABLE" >/dev/null 2>&1 &
else
  echo "Vintage Story 1.22 launcher not found at: $VS_LAUNCHER" >&2
  echo "Vintage Story 1.22 executable not found at: $VS_EXECUTABLE" >&2
  exit 1
fi
