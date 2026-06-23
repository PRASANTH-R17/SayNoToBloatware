# Brand logo overrides

Drop a `<brand>.svg` or `<brand>.png` file here to override the logo used for that
brand when generating default phone images. The brand name must exactly match the
dataset brand slug (e.g. `samsung.svg`, `oneplus.png`, `i-kall.svg`).

The generator (`tools/generate-brand-images/generate.mjs`) checks this folder first
for each brand, then falls back to a `simple-icons` logo, then to brand-name text.
