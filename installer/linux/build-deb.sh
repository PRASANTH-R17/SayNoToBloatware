#!/usr/bin/env bash
#
# Build a Debian package (.deb) for Say No to Bloatware.
#
# Usage:
#   installer/linux/build-deb.sh <version> <publish_dir> <output_dir>
#
# Defaults (relative to repo root):
#   version      1.0.0
#   publish_dir  SNB Desktop/publish/desktop/linux
#   output_dir   dist
#
# Layout produced:
#   /opt/say-no-to-bloatware/            <- self-contained app + assets
#   /usr/bin/say-no-to-bloatware         <- launcher wrapper
#   /usr/share/applications/...desktop   <- menu entry
#   /usr/share/icons/.../256x256/...png  <- icon
#   /lib/udev/rules.d/...                <- Android USB (ADB) permissions
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
cd "$REPO_ROOT"

VERSION="${1:-1.0.0}"
PUBLISH_DIR="${2:-SNB Desktop/publish/desktop/linux}"
OUTPUT_DIR="${3:-dist}"
ICON_SRC="SNB Desktop/src/SNB.Desktop/Assets/app-icon.png"

PKG="say-no-to-bloatware"
APP_BIN="SNB.Desktop"
INSTALL_DIR="/opt/$PKG"

if [ ! -d "$PUBLISH_DIR" ]; then
  echo "error: publish dir not found: $PUBLISH_DIR" >&2
  echo "       run: dotnet publish \"SNB Desktop/src/SNB.Desktop/SNB.Desktop.csproj\" -p:PublishProfile=Linux" >&2
  exit 1
fi

WORK="$(mktemp -d)"
trap 'rm -rf "$WORK"' EXIT
ROOT="$WORK/pkgroot"

UDEV_RULES="installer/linux/51-android-usb.rules"

mkdir -p \
  "$ROOT/DEBIAN" \
  "$ROOT$INSTALL_DIR" \
  "$ROOT/usr/bin" \
  "$ROOT/usr/share/applications" \
  "$ROOT/usr/share/icons/hicolor/256x256/apps" \
  "$ROOT/lib/udev/rules.d"

# App payload.
cp -r "$PUBLISH_DIR/." "$ROOT$INSTALL_DIR/"
chmod +x "$ROOT$INSTALL_DIR/$APP_BIN" "$ROOT$INSTALL_DIR/adb" 2>/dev/null || true

# Launcher wrapper on PATH.
cat > "$ROOT/usr/bin/$PKG" <<EOF
#!/bin/sh
exec "$INSTALL_DIR/$APP_BIN" "\$@"
EOF
chmod +x "$ROOT/usr/bin/$PKG"

# Desktop entry.
cat > "$ROOT/usr/share/applications/$PKG.desktop" <<EOF
[Desktop Entry]
Type=Application
Name=Say No to Bloatware
GenericName=Android Debloater
Comment=Find and remove pre-installed Android bloatware over USB, no root required
Exec=$PKG
Icon=$PKG
Terminal=false
Categories=Utility;System;
Keywords=android;adb;bloatware;debloat;uninstall;
EOF

# Icon.
cp "$ICON_SRC" "$ROOT/usr/share/icons/hicolor/256x256/apps/$PKG.png"

# udev rules so bundled adb can access USB devices without root.
cp "$UDEV_RULES" "$ROOT/lib/udev/rules.d/70-$PKG-android.rules"
chmod 644 "$ROOT/lib/udev/rules.d/70-$PKG-android.rules"

cp "installer/linux/deb-postinst.sh" "$ROOT/DEBIAN/postinst"
chmod 755 "$ROOT/DEBIAN/postinst"

cp "installer/linux/deb-postrm.sh" "$ROOT/DEBIAN/postrm"
chmod 755 "$ROOT/DEBIAN/postrm"

# Installed-size (KB) for control metadata.
INSTALLED_SIZE="$(du -sk "$ROOT$INSTALL_DIR" | cut -f1)"

cat > "$ROOT/DEBIAN/control" <<EOF
Package: $PKG
Version: $VERSION
Section: utils
Priority: optional
Architecture: amd64
Maintainer: Prasanth R <noreply@github.com>
Installed-Size: $INSTALLED_SIZE
Depends: libc6 (>= 2.17), libgcc-s1, libstdc++6, zlib1g
Recommends: libicu-dev | libicu70 | libicu72 | libicu74
Homepage: https://github.com/PRASANTH-R17/SayNoToBloatware
Description: Desktop debloater for Android over ADB
 Say No to Bloatware connects to an Android device over USB (ADB) and helps
 you safely identify and remove or disable pre-installed bloatware. ADB and
 the on-device companion app are bundled; no root is required.
EOF

mkdir -p "$OUTPUT_DIR"
OUT="$OUTPUT_DIR/${PKG}_${VERSION}_amd64.deb"

# Reproducible-ish ownership: everything root:root.
dpkg-deb --root-owner-group --build "$ROOT" "$OUT"

echo "Built: $OUT"
