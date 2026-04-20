#!/usr/bin/env bash
set -euo pipefail

###############################################################################
# Build + Install VSMineralMasonry.GroutTileTextures into Vintage Story 1.21.7
###############################################################################

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
CURRENT_BRANCH="$(git -C "$ROOT_DIR" branch --show-current 2>/dev/null || true)"
TARGET_BRANCH="support/1.21"
CURRENT_HEAD="$(git -C "$ROOT_DIR" rev-parse HEAD 2>/dev/null || true)"
TARGET_HEAD="$(git -C "$ROOT_DIR" rev-parse "$TARGET_BRANCH" 2>/dev/null || true)"
PROJECT_NAME="VSMineralMasonry.GroutTileTextures"
PROJECT_PATH="$ROOT_DIR/$PROJECT_NAME.csproj"
MOD_ID="vsmineralmasonrygrouttiles"

find_worktree_for_branch() {
  local branch_name="$1"

  git -C "$ROOT_DIR" worktree list --porcelain | awk -v target="refs/heads/$branch_name" '
    $1 == "worktree" { wt = $2 }
    $1 == "branch" && $2 == target { print wt; exit }
  '
}

find_worktree_for_commit() {
  local commit_hash="$1"

  git -C "$ROOT_DIR" worktree list --porcelain | awk -v target="$commit_hash" '
    $1 == "worktree" { wt = $2 }
    $1 == "HEAD" && $2 == target { print wt; exit }
  '
}

if [[ "$CURRENT_BRANCH" != "$TARGET_BRANCH" ]]; then
  if [[ -n "$CURRENT_HEAD" && -n "$TARGET_HEAD" && "$CURRENT_HEAD" == "$TARGET_HEAD" ]]; then
    echo "Current checkout already matches $TARGET_BRANCH at $CURRENT_HEAD"
  else
    TARGET_WORKTREE="$(find_worktree_for_branch "$TARGET_BRANCH")"

    if [[ -z "$TARGET_WORKTREE" ]]; then
      TARGET_WORKTREE="$(find_worktree_for_commit "$TARGET_HEAD")"
    fi

    if [[ -z "$TARGET_WORKTREE" ]]; then
      if git -C "$ROOT_DIR" show-ref --verify --quiet "refs/heads/$TARGET_BRANCH"; then
        echo "No separate worktree found. Switching current repo to $TARGET_BRANCH."
        git -C "$ROOT_DIR" checkout "$TARGET_BRANCH"
        exec "$ROOT_DIR/build-install.sh" "$@"
      fi

      echo "ERROR: Could not find worktree or local branch for $TARGET_BRANCH" >&2
      exit 1
    fi

    echo "Switching to $TARGET_BRANCH worktree:"
    echo "  $TARGET_WORKTREE"
    exec "$TARGET_WORKTREE/build-install.sh" "$@"
  fi
fi

cd "$ROOT_DIR"

VS_APP_DIR="${VINTAGE_STORY:-/Applications/Vintage Story 1.21.7.app}"
VS_MODS_DIR="$VS_APP_DIR/Mods"
VS_EXECUTABLE="$VS_APP_DIR/Vintagestory"
VS_DATA_PATH="$HOME/Library/Application Support/VintagestoryData-1.21.7"
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

echo "Deleting installed mod dir: $INSTALL_DIR"
rm -rf "$INSTALL_DIR"

if [[ -e "$INSTALL_DIR" ]]; then
  echo "ERROR: Mod dir still exists: $INSTALL_DIR" >&2
  exit 1
fi

echo "Building $PROJECT_NAME"
VINTAGE_STORY="$VS_APP_DIR" dotnet build "$PROJECT_PATH" -p:NuGetAudit=false

if [[ ! -d "$MOD_BUILD_DIR" ]]; then
  echo "ERROR: Expected build output folder not found: $MOD_BUILD_DIR" >&2
  exit 1
fi

echo "Installing $MOD_ID"
copy_mod "$MOD_BUILD_DIR" "$INSTALL_DIR"

echo
echo "Installed to:"
echo "  $INSTALL_DIR"

if [[ ! -x "$VS_EXECUTABLE" ]]; then
  echo
  echo "Vintage Story 1.21.7 executable not found at: $VS_EXECUTABLE"
  echo "Check that the app bundle contains the Vintagestory executable."
  exit 0
fi

echo
echo "Launching Vintage Story 1.21.7 via:"
echo "  $VS_EXECUTABLE"
echo "Using data path:"
echo "  $VS_DATA_PATH"
mkdir -p "$VS_DATA_PATH"
"$VS_EXECUTABLE" --dataPath "$VS_DATA_PATH" >/dev/null 2>&1 &
