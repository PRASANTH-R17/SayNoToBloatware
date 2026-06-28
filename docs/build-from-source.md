# Build from Source

How to build, run, test, and publish SNB yourself.

## Prerequisites

| Tool | Needed for | Notes |
|------|------------|-------|
| **.NET 10 SDK** | Desktop + CLI + backend | All projects target `net10.0`. There is no `global.json`, so the latest .NET 10 SDK is used. |
| **Flutter SDK** (Dart `^3.11.5`) | Building the bridge APK | Required for local builds; CI builds it automatically for releases. Output goes to `Assets/Bridge/snb_bridge.apk`. |
| **Git** | Cloning | — |

You do **not** need to install Android platform-tools (ADB) separately — the desktop app bundles its
own `adb` binaries for Windows and Linux.

## Repository layout

```
Say No to Bloatware/
├── README.md            ← monorepo overview
├── docs/                ← this documentation
├── Assets/              ← bundled runtime assets (ADB, DB, images, bridge APK)
│   └── Bridge/snb_bridge.apk
├── installer/           ← Windows/Linux packaging scripts
├── SNB Bridge/          ← Flutter Android app (source)
├── SNB Desktop/         ← .NET solution
│   ├── SNB.sln
│   ├── src/
│   │   ├── SNB.Desktop/  ← Avalonia GUI
│   │   ├── SNB.Backend/  ← shared library
│   │   └── SNB.Cli/      ← console front-end
│   └── tests/SNB.Backend.Tests/
└── tools/               ← asset/logo generation scripts
```

## Build and run the desktop app

```bash
# Restore + build the whole solution
dotnet build "SNB Desktop/SNB.sln"

# Run the GUI
dotnet run --project "SNB Desktop/src/SNB.Desktop/SNB.Desktop.csproj"
```

### Run the CLI

The console front-end exercises the same backend (handy for quick testing):

```bash
dotnet run --project "SNB Desktop/src/SNB.Cli/SNB.Cli.csproj"
```

It detects devices, prepares the selected one, and offers a simple menu (list apps, recommended
removals, apps with alternatives, etc.).

## Run the tests

```bash
dotnet test "SNB Desktop/tests/SNB.Backend.Tests/SNB.Backend.Tests.csproj"
```

## Publish a distributable build

Publish profiles produce self-contained builds:

```bash
# Windows folder publish (win-x64, self-contained) -> publish/desktop/win-portable/
dotnet publish "SNB Desktop/src/SNB.Desktop/SNB.Desktop.csproj" -p:PublishProfile=WindowsPortable

# Windows single-file (win-x64, self-contained) -> publish/desktop/win-single/SNB.Desktop.exe
dotnet publish "SNB Desktop/src/SNB.Desktop/SNB.Desktop.csproj" -p:PublishProfile=WindowsSingleFile

# Linux
dotnet publish "SNB Desktop/src/SNB.Desktop/SNB.Desktop.csproj" -p:PublishProfile=Linux
```

The CLI has matching `Windows`/`Linux` publish profiles under
`SNB Desktop/src/SNB.Cli/Properties/PublishProfiles/`.

## How bundled assets are wired

The desktop and CLI `.csproj` files include the runtime assets as `Content` items with
`CopyToOutputDirectory`/`CopyToPublishDirectory=PreserveNewest`, linking them next to the binary:

- `Assets/ADB/Windows/adb.exe` (+ `AdbWinApi.dll`, `AdbWinUsbApi.dll`) and `Assets/ADB/Linux/adb`
- `Assets/Bridge/snb_bridge.apk` → `Bridge/snb_bridge.apk`
- `Assets/Database/snb.db` → `Default/snb.db`
- `Assets/Images/*.png` → `Assets/Images/`

At runtime these are resolved relative to the app's base directory (e.g. the bridge APK at
`Bridge/snb_bridge.apk`), so a published build is fully self-contained.

## Rebuilding the bridge APK (optional)

The desktop deploys `Bridge/snb_bridge.apk`. To rebuild it from the Flutter source:

```bash
cd "SNB Bridge"
flutter pub get
flutter build apk --release
```

The output is `SNB Bridge/build/app/outputs/flutter-apk/app-release.apk`. Copy it to
`Assets/Bridge/snb_bridge.apk` (the path the desktop `.csproj` bundles from) so the next build picks up
the new APK. If you bump the bridge's version:

1. Update the `version:` build number in [`SNB Bridge/pubspec.yaml`](../SNB%20Bridge/pubspec.yaml)
   (the number after `+` is the Android `versionCode`).
2. Set the matching `BridgeVersionCode` in
   [`SnbBackendOptions.cs`](../SNB%20Desktop/src/SNB.Backend/DependencyInjection/SnbBackendOptions.cs)
   so the desktop re-installs the APK when the bundled build is newer.

See [SNB Bridge/README.md](../SNB%20Bridge/README.md) for the bridge's HTTP API and dashboard details.

## Generating brand/device images (optional)

The `tools/` folder contains helper scripts:

- `tools/slice_logos.py` / `tools/export_assets.py` — produce app/launcher icons from master logos.
- `tools/generate-brand-images/` — a Node project (`sharp` + `simple-icons`) that generates the
  per-brand device images under `Assets/Images/`. From that folder: `npm install && npm run generate`.
- `tools/brand-logos/` — drop-in folder for custom brand logo overrides (see its README).
