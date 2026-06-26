#!/bin/sh
set -e

# Reload udev so newly installed Android USB rules take effect.
if command -v udevadm >/dev/null 2>&1; then
  udevadm control --reload-rules 2>/dev/null || true
  udevadm trigger --subsystem-match=usb --action=add 2>/dev/null || true
fi

# Remind Debian/Ubuntu users in the plugdev group to refresh their session.
if getent group plugdev >/dev/null 2>&1; then
  if ! id -nG 2>/dev/null | tr ' ' '\n' | grep -qx plugdev; then
    echo "Note: add yourself to the plugdev group for USB access, then log out and back in:"
    echo "  sudo usermod -aG plugdev \$USER"
  fi
fi

exit 0
