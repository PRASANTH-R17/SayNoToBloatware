#!/usr/bin/env bash
#
# Build a portable AppImage for Say No to Bloatware.
#
# Usage:
#   installer/linux/build-appimage.sh <version> <publish_dir> <output_dir>
#
# Defaults (relative to repo root):
#   version      1.0.0
#   publish_dir  SNB Desktop/publish/desktop/linux
#   output_dir   dist
#
# Requires: a Linux host (or WSL) with curl + FUSE (or appimagetool's --appimage-extract-and-run).
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
cd "$REPO_ROOT"

VERSION="${1:-1.0.0}"
PUBLISH_DIR="${2:-SNB Desktop/publish/desktop/linux}"
OUTPUT_DIR="${3:-dist}"
ICON_SRC="SNB Desktop/src/SNB.Desktop/Assets/app-icon.png"
APP_BIN="SNB.Desktop"

if [ ! -d "$PUBLISH_DIR" ]; then
  echo "error: publish dir not found: $PUBLISH_DIR" >&2
  echo "       run: dotnet publish \"SNB Desktop/src/SNB.Desktop/SNB.Desktop.csproj\" -p:PublishProfile=Linux" >&2
  exit 1
fi

WORK="$(mktemp -d)"
trap 'rm -rf "$WORK"' EXIT
APPDIR="$WORK/SNB.AppDir"

mkdir -p "$APPDIR/usr/bin" "$APPDIR/usr/share/applications" "$APPDIR/usr/share/icons/hicolor/256x256/apps"

# Bundle the published, self-contained app.
cp -r "$PUBLISH_DIR/." "$APPDIR/usr/bin/"
chmod +x "$APPDIR/usr/bin/$APP_BIN" "$APPDIR/usr/bin/adb" 2>/dev/null || true

# AppRun entrypoint.
cp "installer/linux/AppRun" "$APPDIR/AppRun"
chmod +x "$APPDIR/AppRun"

# Desktop entry (required at AppDir root + standard location).
cp "installer/linux/snb.desktop" "$APPDIR/snb.desktop"
cp "installer/linux/snb.desktop" "$APPDIR/usr/share/applications/snb.desktop"

# Icon (required at AppDir root as <icon-name>.png + standard location).
cp "$ICON_SRC" "$APPDIR/snb.png"
cp "$ICON_SRC" "$APPDIR/usr/share/icons/hicolor/256x256/apps/snb.png"

# Fetch appimagetool if it isn't already cached.
TOOL="$WORK/appimagetool.AppImage"
curl -fsSL -o "$TOOL" \
  "https://github.com/AppImage/appimagetool/releases/download/continuous/appimagetool-x86_64.AppImage"
chmod +x "$TOOL"

mkdir -p "$OUTPUT_DIR"
OUT="$OUTPUT_DIR/SNB-${VERSION}-x86_64.AppImage"

# --appimage-extract-and-run avoids needing FUSE on CI runners.
ARCH=x86_64 "$TOOL" --appimage-extract-and-run "$APPDIR" "$OUT"

echo "Built: $OUT"
