#!/usr/bin/env bash
#
# Build an RPM package for Say No to Bloatware (Fedora/RHEL/Rocky/Alma).
#
# Usage:
#   installer/linux/build-rpm.sh <version> <publish_dir> <output_dir>
#
# Defaults (relative to repo root):
#   version      1.0.0
#   publish_dir  SNB Desktop/publish/desktop/linux
#   output_dir   dist
#
# Layout produced (same as .deb):
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
UDEV_RULES="installer/linux/51-android-usb.rules"
SPEC_SRC="installer/linux/say-no-to-bloatware.spec"

if [ ! -d "$PUBLISH_DIR" ]; then
  echo "error: publish dir not found: $PUBLISH_DIR" >&2
  echo "       run: dotnet publish \"SNB Desktop/src/SNB.Desktop/SNB.Desktop.csproj\" -p:PublishProfile=Linux" >&2
  exit 1
fi

if ! command -v rpmbuild >/dev/null 2>&1; then
  echo "error: rpmbuild not found (install the rpm package)" >&2
  exit 1
fi

WORK="$(mktemp -d)"
trap 'rm -rf "$WORK"' EXIT
RPMTOP="$WORK/rpm"

mkdir -p \
  "$RPMTOP/BUILD" \
  "$RPMTOP/RPMS" \
  "$RPMTOP/SOURCES" \
  "$RPMTOP/SPECS" \
  "$RPMTOP/SRPMS" \
  "$RPMTOP/BUILDROOT"

cp -r "$PUBLISH_DIR/." "$RPMTOP/SOURCES/payload/"
cp "$ICON_SRC" "$RPMTOP/SOURCES/app-icon.png"
cp "$UDEV_RULES" "$RPMTOP/SOURCES/51-android-usb.rules"

cp "$SPEC_SRC" "$RPMTOP/SPECS/"
cp "installer/linux/deb-postinst.sh" "$RPMTOP/SPECS/"
cp "installer/linux/deb-postrm.sh" "$RPMTOP/SPECS/"

rpmbuild -bb \
  --define "_topdir $RPMTOP" \
  --define "version $VERSION" \
  "$RPMTOP/SPECS/say-no-to-bloatware.spec"

mapfile -t BUILT_RPMS < <(find "$RPMTOP/RPMS/x86_64" -maxdepth 1 -name "${PKG}-${VERSION}-1*.rpm" -type f 2>/dev/null)
if [ "${#BUILT_RPMS[@]}" -eq 0 ]; then
  echo "error: expected RPM not found under $RPMTOP/RPMS/x86_64/" >&2
  exit 1
fi

mkdir -p "$OUTPUT_DIR"
OUT="$OUTPUT_DIR/${PKG}-${VERSION}-1.x86_64.rpm"
cp "${BUILT_RPMS[0]}" "$OUT"

echo "Built: $OUT"
