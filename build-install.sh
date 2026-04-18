#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_NAME="VSMineralMasonry.GroutTileTextures"
PROJECT_PATH="$ROOT_DIR/$PROJECT_NAME.csproj"
MOD_ID="vsmineralmasonrygrouttiles"
VS_APP_DIR="${VINTAGE_STORY:-/Applications/Vintage Story.app}"
VS_MODS_DIR="$VS_APP_DIR/Mods"
MOD_BUILD_DIR="$ROOT_DIR/bin/Debug/Mods/mod"
INSTALL_DIR="$VS_MODS_DIR/$MOD_ID"

if ! command -v dotnet >/dev/null 2>&1; then
  echo "dotnet is not installed or not on PATH." >&2
  exit 1
fi

if [[ ! -d "$VS_APP_DIR" ]]; then
  echo "ERROR: Vintage Story app not found: $VS_APP_DIR" >&2
  exit 1
fi

copy_mod() {
  local source_dir="$1"
  local target_dir="$2"

  if [[ ! -w "$VS_MODS_DIR" ]]; then
    sudo mkdir -p "$VS_MODS_DIR"
    sudo rm -rf "$target_dir"
    sudo cp -R "$source_dir" "$target_dir"
  else
    mkdir -p "$VS_MODS_DIR"
    rm -rf "$target_dir"
    cp -R "$source_dir" "$target_dir"
  fi
}

rm -rf "$ROOT_DIR/bin" "$ROOT_DIR/obj"

echo "Building $PROJECT_NAME"
VINTAGE_STORY="$VS_APP_DIR" dotnet build "$PROJECT_PATH" -p:NuGetAudit=false

if [[ ! -d "$MOD_BUILD_DIR" ]]; then
  echo "ERROR: Expected build output folder not found: $MOD_BUILD_DIR" >&2
  exit 1
fi

echo "Installing $MOD_ID"
copy_mod "$MOD_BUILD_DIR" "$INSTALL_DIR"

echo "Installed to:"
echo "  $INSTALL_DIR"
