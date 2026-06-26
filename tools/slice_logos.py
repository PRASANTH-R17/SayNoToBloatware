"""Slice the combined 3-logo image into clean, square, transparent master PNGs.

Input  : a single PNG with 3 logos on a black background (each with a text
         label underneath).
Output : tools/logos/snb.png, snb-desktop.png, snb-bridge.png  (1024x1024 RGBA)

The black background is made transparent, the text label band under each glyph
is dropped, the glyph is auto-trimmed and centered onto a square canvas.
"""
from __future__ import annotations

import os
import sys

from PIL import Image

HERE = os.path.dirname(os.path.abspath(__file__))
OUT_DIR = os.path.join(HERE, "logos")

# Pixels whose max channel is below this are treated as "background" (black).
BG_THRESHOLD = 28
# A row counts as "content" if it has at least this many non-background pixels.
ROW_MIN_PIXELS = 4
MASTER_SIZE = 1024
PAD_RATIO = 0.06  # transparent padding around the glyph as a fraction of size


def luminance_mask(img: Image.Image):
    """Return (pixels, width, height) where pixels[y][x] is True for content."""
    rgb = img.convert("RGB")
    w, h = rgb.size
    px = rgb.load()
    mask = [[False] * w for _ in range(h)]
    for y in range(h):
        row = mask[y]
        for x in range(w):
            r, g, b = px[x, y]
            if max(r, g, b) > BG_THRESHOLD:
                row[x] = True
    return mask, w, h


def content_rows(mask, w, h):
    rows = []
    for y in range(h):
        count = sum(1 for x in range(w) if mask[y][x])
        rows.append(count >= ROW_MIN_PIXELS)
    return rows


def row_clusters(rows):
    """Return list of (start, end_exclusive) vertical content bands."""
    clusters = []
    start = None
    for y, on in enumerate(rows):
        if on and start is None:
            start = y
        elif not on and start is not None:
            clusters.append((start, y))
            start = None
    if start is not None:
        clusters.append((start, len(rows)))
    return clusters


def bbox_of_region(mask, w, x0, x1, y0, y1):
    minx, miny, maxx, maxy = x1, y1, x0, y0
    found = False
    for y in range(y0, y1):
        for x in range(x0, x1):
            if mask[y][x]:
                found = True
                if x < minx:
                    minx = x
                if x > maxx:
                    maxx = x
                if y < miny:
                    miny = y
                if y > maxy:
                    maxy = y
    if not found:
        return None
    return (minx, miny, maxx + 1, maxy + 1)


def make_transparent(crop: Image.Image) -> Image.Image:
    crop = crop.convert("RGBA")
    px = crop.load()
    w, h = crop.size
    for y in range(h):
        for x in range(w):
            r, g, b, a = px[x, y]
            if max(r, g, b) <= BG_THRESHOLD:
                px[x, y] = (r, g, b, 0)
    return crop


def square_canvas(glyph: Image.Image) -> Image.Image:
    inner = int(MASTER_SIZE * (1 - 2 * PAD_RATIO))
    gw, gh = glyph.size
    scale = min(inner / gw, inner / gh)
    new = (max(1, int(gw * scale)), max(1, int(gh * scale)))
    glyph = glyph.resize(new, Image.LANCZOS)
    canvas = Image.new("RGBA", (MASTER_SIZE, MASTER_SIZE), (0, 0, 0, 0))
    canvas.paste(glyph, ((MASTER_SIZE - new[0]) // 2, (MASTER_SIZE - new[1]) // 2), glyph)
    return canvas


def slice_third(img, mask, w, h, x0, x1, name):
    rows = content_rows([row[x0:x1] for row in mask], x1 - x0, h)
    clusters = row_clusters(rows)
    if not clusters:
        raise RuntimeError(f"No content found in third '{name}'")
    # Largest cluster by height = the glyph (text label is a shorter band).
    glyph_band = max(clusters, key=lambda c: c[1] - c[0])
    gy0, gy1 = glyph_band
    bbox = bbox_of_region(mask, w, x0, x1, gy0, gy1)
    if bbox is None:
        raise RuntimeError(f"Empty glyph bbox for '{name}'")
    crop = img.crop(bbox)
    crop = make_transparent(crop)
    out = square_canvas(crop)
    path = os.path.join(OUT_DIR, f"{name}.png")
    out.save(path)
    print(f"  {name}.png  bbox={bbox}  band={glyph_band}")
    return path


def main():
    if len(sys.argv) < 2:
        print("usage: python slice_logos.py <combined.png>")
        sys.exit(1)
    src = sys.argv[1]
    os.makedirs(OUT_DIR, exist_ok=True)
    img = Image.open(src).convert("RGB")
    mask, w, h = luminance_mask(img)
    third = w // 3
    bounds = [(0, third), (third, 2 * third), (2 * third, w)]
    names = ["snb", "snb-desktop", "snb-bridge"]
    print(f"image {w}x{h} -> {OUT_DIR}")
    for (x0, x1), name in zip(bounds, names):
        slice_third(img, mask, w, h, x0, x1, name)
    print("done")


if __name__ == "__main__":
    main()
