// Generates per-brand default phone images (phone frame + brand logo or brand-name
// text) for the "Say No to Bloatware" app.
//
// For each brand we:
//   1. Check tools/brand-logos/<brand>.svg|png for a manual override logo.
//   2. Otherwise look up a logo in simple-icons by slug.
//   3. If a logo is found, tint it to a readable color and composite it centered in
//      the phone screen area.
//   4. Otherwise render the brand display name as centered, word-wrapped text.
//
// Output: Assets/Images/default-<brand>.png for all brands plus a generic
// Assets/Images/default-phone.png used as the final fallback.
//
// Re-runnable: `npm run generate` (or `node generate.mjs`) from this folder.

import { fileURLToPath } from "node:url";
import path from "node:path";
import fs from "node:fs";
import sharp from "sharp";
import * as simpleIcons from "simple-icons";

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

// tools/generate-brand-images -> tools -> workspace root
const workspaceRoot = path.resolve(__dirname, "..", "..");
const overrideDir = path.resolve(__dirname, "..", "brand-logos");
const outputDir = path.join(workspaceRoot, "Assets", "Images");

// ---------------------------------------------------------------------------
// Brand list (37, exact dataset slugs) + display-name overrides.
// ---------------------------------------------------------------------------
const BRANDS = [
  "acer", "ai-plus", "alcatel", "apple", "asus", "cmf", "google", "hmd",
  "honor", "huawei", "i-kall", "infinix", "iqoo", "itel", "jio", "jiophone",
  "lava", "moto", "motorola", "nokia", "nothing", "nubia", "oneplus", "oppo",
  "poco", "realme", "redmi", "samsung", "sony", "spark", "tcl", "tecno",
  "vivo", "wobble", "xiaomi", "zenfone", "zte",
];

const DISPLAY_NAME_OVERRIDES = {
  "ai-plus": "Ai+",
  iqoo: "iQOO",
  hmd: "HMD",
  tcl: "TCL",
  zte: "ZTE",
  cmf: "CMF",
  jiophone: "JioPhone",
  oneplus: "OnePlus",
  poco: "POCO",
  "i-kall": "i-Kall",
};

// Map a dataset brand slug to candidate simple-icons slugs (first match wins).
const SIMPLE_ICON_SLUGS = {
  moto: "motorola",
  zenfone: "asus",
  redmi: "xiaomi",
};

function displayName(brand) {
  if (DISPLAY_NAME_OVERRIDES[brand]) return DISPLAY_NAME_OVERRIDES[brand];
  return brand
    .split("-")
    .map((w) => (w ? w[0].toUpperCase() + w.slice(1) : w))
    .join(" ");
}

// ---------------------------------------------------------------------------
// simple-icons lookup by slug.
// ---------------------------------------------------------------------------
const iconsBySlug = {};
for (const key of Object.keys(simpleIcons)) {
  const icon = simpleIcons[key];
  if (icon && typeof icon === "object" && icon.slug && icon.path) {
    iconsBySlug[icon.slug] = icon;
  }
}

function findSimpleIcon(brand) {
  const candidates = [
    SIMPLE_ICON_SLUGS[brand],
    brand,
    brand.replace(/-/g, ""),
  ].filter(Boolean);
  for (const c of candidates) {
    if (iconsBySlug[c]) return iconsBySlug[c];
  }
  return null;
}

// ---------------------------------------------------------------------------
// Geometry: 600 x 1200 phone frame, screen centered on the canvas center.
// ---------------------------------------------------------------------------
const W = 600;
const H = 1200;
const CX = W / 2;
const CY = H / 2;

const SCREEN = { x: 64, y: 80, w: 472, h: 1040, rx: 56 };
const LOGO_BOX = 340; // max logo size (square) inside the screen

function phoneFrameSvg() {
  return `<?xml version="1.0" encoding="UTF-8"?>
<svg width="${W}" height="${H}" viewBox="0 0 ${W} ${H}" xmlns="http://www.w3.org/2000/svg">
  <defs>
    <linearGradient id="body" x1="0" y1="0" x2="0" y2="1">
      <stop offset="0" stop-color="#2b2f36"/>
      <stop offset="1" stop-color="#15171c"/>
    </linearGradient>
    <linearGradient id="screen" x1="0" y1="0" x2="0" y2="1">
      <stop offset="0" stop-color="#fbfbfd"/>
      <stop offset="1" stop-color="#ececed"/>
    </linearGradient>
  </defs>
  <!-- phone body / bezel -->
  <rect x="40" y="40" width="520" height="1120" rx="84" ry="84" fill="url(#body)"/>
  <rect x="48" y="48" width="504" height="1104" rx="78" ry="78" fill="none" stroke="#3a3f47" stroke-width="2"/>
  <!-- screen -->
  <rect x="${SCREEN.x}" y="${SCREEN.y}" width="${SCREEN.w}" height="${SCREEN.h}" rx="${SCREEN.rx}" ry="${SCREEN.rx}" fill="url(#screen)"/>
  <!-- notch + camera dot -->
  <rect x="${CX - 70}" y="96" width="140" height="26" rx="13" ry="13" fill="#15171c"/>
  <circle cx="${CX + 78}" cy="109" r="7" fill="#2b2f36"/>
</svg>`;
}

// Relative luminance (0..1) of a hex color.
function luminance(hex) {
  const n = parseInt(hex, 16);
  const r = ((n >> 16) & 255) / 255;
  const g = ((n >> 8) & 255) / 255;
  const b = (n & 255) / 255;
  const lin = (c) => (c <= 0.03928 ? c / 12.92 : ((c + 0.055) / 1.055) ** 2.4);
  return 0.2126 * lin(r) + 0.7152 * lin(g) + 0.0722 * lin(b);
}

// Pick a logo color that stays readable on the light screen.
function readableColor(hex) {
  if (!hex) return "#1d1d1f";
  const lum = luminance(hex);
  if (lum > 0.7) return "#1d1d1f"; // too light against light screen
  return `#${hex}`;
}

function logoSvgFromIcon(icon) {
  const color = readableColor(icon.hex);
  return `<?xml version="1.0" encoding="UTF-8"?>
<svg width="${LOGO_BOX}" height="${LOGO_BOX}" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg">
  <path d="${icon.path}" fill="${color}"/>
</svg>`;
}

// ---------------------------------------------------------------------------
// Word-wrapped centered text rendering.
// ---------------------------------------------------------------------------
function escapeXml(s) {
  return s.replace(/[<>&'"]/g, (c) =>
    ({ "<": "&lt;", ">": "&gt;", "&": "&amp;", "'": "&apos;", '"': "&quot;" }[c])
  );
}

function wrapText(text, maxCharsPerLine) {
  const words = text.split(/\s+/).filter(Boolean);
  const lines = [];
  let current = "";
  for (const word of words) {
    const candidate = current ? `${current} ${word}` : word;
    if (candidate.length > maxCharsPerLine && current) {
      lines.push(current);
      current = word;
    } else {
      current = candidate;
    }
  }
  if (current) lines.push(current);
  return lines.length ? lines : [text];
}

function textSvg(name) {
  const innerWidth = SCREEN.w - 64;
  // Scale font so the longest single word fits the screen width.
  const longestWord = name.split(/\s+/).reduce((a, b) => (b.length > a.length ? b : a), "");
  let fontSize = 96;
  const maxByWord = Math.floor((innerWidth / (longestWord.length || 1)) / 0.62);
  fontSize = Math.max(40, Math.min(fontSize, maxByWord));

  const charsPerLine = Math.max(1, Math.floor(innerWidth / (fontSize * 0.62)));
  const lines = wrapText(name, charsPerLine);
  const lineHeight = fontSize * 1.18;
  const totalHeight = lines.length * lineHeight;
  const startY = CY - totalHeight / 2 + fontSize * 0.82;

  const tspans = lines
    .map((line, i) =>
      `<tspan x="${CX}" y="${(startY + i * lineHeight).toFixed(1)}">${escapeXml(line)}</tspan>`
    )
    .join("\n      ");

  return `<?xml version="1.0" encoding="UTF-8"?>
<svg width="${W}" height="${H}" viewBox="0 0 ${W} ${H}" xmlns="http://www.w3.org/2000/svg">
  <style>
    text { font-family: 'Segoe UI', 'Helvetica Neue', Arial, sans-serif; font-weight: 700; }
  </style>
  <text text-anchor="middle" font-size="${fontSize}" fill="#1d1d1f">
      ${tspans}
  </text>
</svg>`;
}

// Generic phone glyph for the final fallback image.
function phoneGlyphSvg() {
  const gw = 200;
  const gh = 360;
  const gx = CX - gw / 2;
  const gy = CY - gh / 2;
  return `<?xml version="1.0" encoding="UTF-8"?>
<svg width="${W}" height="${H}" viewBox="0 0 ${W} ${H}" xmlns="http://www.w3.org/2000/svg">
  <rect x="${gx}" y="${gy}" width="${gw}" height="${gh}" rx="36" ry="36" fill="none" stroke="#1d1d1f" stroke-width="12"/>
  <circle cx="${CX}" cy="${gy + gh - 36}" r="12" fill="#1d1d1f"/>
  <rect x="${CX - 30}" y="${gy + 22}" width="60" height="10" rx="5" ry="5" fill="#1d1d1f"/>
</svg>`;
}

// ---------------------------------------------------------------------------
// Compositing helpers.
// ---------------------------------------------------------------------------
async function renderLogoBuffer(source) {
  // source: { type: 'svg'|'png', buffer } -> PNG buffer resized to fit LOGO_BOX.
  return sharp(source.buffer)
    .resize(LOGO_BOX, LOGO_BOX, {
      fit: "contain",
      background: { r: 0, g: 0, b: 0, alpha: 0 },
    })
    .png()
    .toBuffer();
}

async function compositeCentered(frameBuffer, overlayBuffer) {
  const overlay = sharp(overlayBuffer);
  const meta = await overlay.metadata();
  const left = Math.round(CX - meta.width / 2);
  const top = Math.round(CY - meta.height / 2);
  return sharp(frameBuffer)
    .composite([{ input: overlayBuffer, left, top }])
    .png()
    .toBuffer();
}

async function compositeSvgOverlay(frameBuffer, svg) {
  // SVGs here are already full-canvas (W x H), so composite at 0,0.
  return sharp(frameBuffer)
    .composite([{ input: Buffer.from(svg), top: 0, left: 0 }])
    .png()
    .toBuffer();
}

// ---------------------------------------------------------------------------
// Override-folder lookup.
// ---------------------------------------------------------------------------
function findOverride(brand) {
  for (const ext of ["svg", "png"]) {
    const p = path.join(overrideDir, `${brand}.${ext}`);
    if (fs.existsSync(p)) return { type: ext, file: p };
  }
  return null;
}

// ---------------------------------------------------------------------------
// Main.
// ---------------------------------------------------------------------------
async function main() {
  fs.mkdirSync(outputDir, { recursive: true });

  const frameBuffer = await sharp(Buffer.from(phoneFrameSvg())).png().toBuffer();

  const usedLogo = [];
  const usedText = [];

  for (const brand of BRANDS) {
    const outPath = path.join(outputDir, `default-${brand}.png`);
    let result;
    let mode;

    const override = findOverride(brand);
    if (override) {
      const buffer = fs.readFileSync(override.file);
      const logo = await renderLogoBuffer({ type: override.type, buffer });
      result = await compositeCentered(frameBuffer, logo);
      mode = `logo (override: ${path.basename(override.file)})`;
      usedLogo.push(brand);
    } else {
      const icon = findSimpleIcon(brand);
      if (icon) {
        const logoSvg = logoSvgFromIcon(icon);
        const logo = await renderLogoBuffer({ type: "svg", buffer: Buffer.from(logoSvg) });
        result = await compositeCentered(frameBuffer, logo);
        mode = `logo (simple-icons: ${icon.slug})`;
        usedLogo.push(brand);
      } else {
        const svg = textSvg(displayName(brand));
        result = await compositeSvgOverlay(frameBuffer, svg);
        mode = `text ("${displayName(brand)}")`;
        usedText.push(brand);
      }
    }

    fs.writeFileSync(outPath, result);
    console.log(`  ${brand.padEnd(12)} -> ${mode}`);
  }

  // Generic final-fallback image: use the Android logo, falling back to a plain
  // phone glyph only if the icon is unavailable.
  const androidIcon = iconsBySlug["android"];
  let generic;
  if (androidIcon) {
    const logoSvg = logoSvgFromIcon(androidIcon);
    const logo = await renderLogoBuffer({ type: "svg", buffer: Buffer.from(logoSvg) });
    generic = await compositeCentered(frameBuffer, logo);
    console.log(`  ${"default".padEnd(12)} -> logo (simple-icons: android)`);
  } else {
    generic = await compositeSvgOverlay(frameBuffer, phoneGlyphSvg());
    console.log(`  ${"default".padEnd(12)} -> glyph (android icon unavailable)`);
  }
  fs.writeFileSync(path.join(outputDir, "default-phone.png"), generic);

  console.log("\nSummary");
  console.log(`  Output dir : ${outputDir}`);
  console.log(`  Brands     : ${BRANDS.length}`);
  console.log(`  Real logo  : ${usedLogo.length} -> ${usedLogo.join(", ")}`);
  console.log(`  Text only  : ${usedText.length} -> ${usedText.join(", ")}`);
  console.log(`  Plus default-phone.png (generic fallback).`);
}

main().catch((err) => {
  console.error(err);
  process.exit(1);
});
