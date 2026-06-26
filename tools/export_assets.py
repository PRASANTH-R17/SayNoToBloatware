"""Generate all derived icon/logo assets from the sliced master PNGs.

Run after slice_logos.py. Writes:
  Desktop in-app brand : SNB.Desktop/Assets/logo.png, brand-logo.png  (from snb)
  Desktop OS/EXE icon  : SNB.Desktop/Assets/app.ico                   (from snb-desktop)
  Bridge launcher icon : android mipmap-*/ic_launcher.png (5 densities, snb-bridge)
"""
from __future__ import annotations

import os

from PIL import Image

HERE = os.path.dirname(os.path.abspath(__file__))
REPO = os.path.dirname(HERE)
LOGOS = os.path.join(HERE, "logos")

DESKTOP_ASSETS = os.path.join(REPO, "SNB Desktop", "src", "SNB.Desktop", "Assets")
BRIDGE_RES = os.path.join(
    REPO, "SNB Bridge", "android", "app", "src", "main", "res"
)


def load(name: str) -> Image.Image:
    return Image.open(os.path.join(LOGOS, f"{name}.png")).convert("RGBA")


def save_png(img: Image.Image, path: str, size: int):
    os.makedirs(os.path.dirname(path), exist_ok=True)
    img.resize((size, size), Image.LANCZOS).save(path)
    print(f"  wrote {path}  ({size}x{size})")


def export_desktop_brand():
    snb = load("snb")
    save_png(snb, os.path.join(DESKTOP_ASSETS, "logo.png"), 512)
    save_png(snb, os.path.join(DESKTOP_ASSETS, "brand-logo.png"), 512)


def export_desktop_icon():
    snb_desktop = load("snb-desktop")
    ico_path = os.path.join(DESKTOP_ASSETS, "app.ico")
    sizes = [(s, s) for s in (16, 24, 32, 48, 64, 128, 256)]
    snb_desktop.save(ico_path, format="ICO", sizes=sizes)
    print(f"  wrote {ico_path}  ({', '.join(str(s) for s, _ in sizes)})")
    save_png(snb_desktop, os.path.join(DESKTOP_ASSETS, "app-icon.png"), 256)


def export_bridge_launcher():
    bridge = load("snb-bridge")
    densities = {
        "mipmap-mdpi": 48,
        "mipmap-hdpi": 72,
        "mipmap-xhdpi": 96,
        "mipmap-xxhdpi": 144,
        "mipmap-xxxhdpi": 192,
    }
    for folder, size in densities.items():
        path = os.path.join(BRIDGE_RES, folder, "ic_launcher.png")
        save_png(bridge, path, size)


def main():
    print("Desktop brand:")
    export_desktop_brand()
    print("Desktop icon:")
    export_desktop_icon()
    print("Bridge launcher:")
    export_bridge_launcher()
    print("done")


if __name__ == "__main__":
    main()
