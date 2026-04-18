#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_NAME="VSMineralMasonry.GroutTileTextures"
PROJECT_PATH="$ROOT_DIR/$PROJECT_NAME.csproj"
MOD_OUTPUT_DIR="$ROOT_DIR/bin/Release/Mods/mod"
VERSION_FILE="$ROOT_DIR/VERSION"
DIST_DIR="$ROOT_DIR/dist"

if ! command -v dotnet >/dev/null 2>&1; then
  echo "dotnet is not installed or not on PATH." >&2
  exit 1
fi

if ! command -v zip >/dev/null 2>&1; then
  echo "zip is not installed or not on PATH." >&2
  exit 1
fi

if [[ ! -f "$VERSION_FILE" ]]; then
  echo "VERSION file not found at $VERSION_FILE" >&2
  exit 1
fi

VERSION="$(tr -d '[:space:]' < "$VERSION_FILE")"
if [[ -z "$VERSION" ]]; then
  echo "Could not determine mod version from VERSION file" >&2
  exit 1
fi

echo "Building $PROJECT_NAME $VERSION"
dotnet build "$PROJECT_PATH" -c Release -p:NuGetAudit=false

if [[ ! -f "$MOD_OUTPUT_DIR/$PROJECT_NAME.dll" ]]; then
  echo "Expected built DLL not found in $MOD_OUTPUT_DIR" >&2
  exit 1
fi

if [[ ! -f "$MOD_OUTPUT_DIR/modinfo.json" ]]; then
  echo "Expected modinfo.json not found in $MOD_OUTPUT_DIR" >&2
  exit 1
fi

mkdir -p "$DIST_DIR"
ZIP_PATH="$DIST_DIR/${PROJECT_NAME}-${VERSION}.zip"
rm -f "$ZIP_PATH"

(
  cd "$MOD_OUTPUT_DIR"
  zip -r "$ZIP_PATH" . -x '*.pdb' >/dev/null
)

echo "Created release package:"
echo "  $ZIP_PATH"
