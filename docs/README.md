<div align="center">

<img src="brand/snb.png" alt="Say No to Bloatware" width="120" />

# Say No to Bloatware — Documentation

Clean your Android device. Remove bloatware. Take control.

</div>

---

**Say No to Bloatware (SNB)** is a desktop application that helps you safely identify and remove
unwanted or pre-installed "bloatware" apps from your Android device over a USB connection. It pairs
a Windows/Linux desktop client with a lightweight on-device companion app and talks to the phone
through ADB — no root required.

## What's in the box

| Component | What it is |
|-----------|------------|
| **SNB Desktop** | The cross-platform desktop client (Avalonia / .NET 10) you interact with. |
| **SNB Backend** | Shared library that drives ADB, the on-device bridge, the bloatware database, and device matching. |
| **SNB Bridge** | A small Android app (Flutter) that runs a local HTTP server on the device to expose rich app metadata and icons. |

## Documentation index

| Page | For whom | Contents |
|------|----------|----------|
| [Getting Started](getting-started.md) | Users | Requirements, enabling USB debugging, first run, first scan. |
| [User Guide](user-guide.md) | Users | Full walkthrough: device selection, scanning, removal, export, settings. |
| [Architecture](architecture.md) | Developers | How the pieces fit together, data flow, and data stores. |
| [Build from Source](build-from-source.md) | Developers | Building, running, testing, and publishing the apps. |
| [Releasing & Deployment](releasing.md) | Developers | Packaging Windows (portable + installer) and Linux (AppImage + .deb), and the release pipeline. |
| [Troubleshooting](troubleshooting.md) | Everyone | Fixes for common connection and removal issues. |
| [Bridge HTTP API](../SNB%20Bridge/README.md) | Developers | The on-device bridge's full HTTP endpoint reference. |

## Key features

- **Automatic device detection** over ADB, with a matched photo of your phone.
- **Bloatware detection** that classifies installed apps into *All*, *Recommended Removal*, and *Apps with Alternatives*.
- **Safe removal** for the current user, with a clear confirmation → progress → summary flow.
- **Disable fallback**: when a manufacturer blocks uninstalling a protected app, SNB tries to disable it instead and reports the real outcome (Removed / Disabled / Failed).
- **Export** the current app list to CSV or JSON.
- **Light/Dark themes** and **English/Tamil** language, both remembered across restarts.
- **Bundled tooling**: ADB and the bridge APK ship with the app — nothing to install manually.

## Safety first

SNB removes apps **for the current user** (`pm uninstall --user 0`) rather than wiping them from
the system partition, and never requires root. Even so, removing system components can affect device
behaviour. Stick to the *Recommended Removal* list unless you know what a package does, and see
[Troubleshooting](troubleshooting.md) if something looks off.
