# Say No to Bloatware

Monorepo for the **Say No to Bloatware** debloater ecosystem — tools to inspect and manage bloatware on Android devices from Windows.

## Subprojects

| Folder | Description |
|--------|-------------|
| [SNB Bridge](SNB%20Bridge/) | Android bridge app — runs an HTTP server on the device and exposes installed-app data over ADB port forwarding. See its [README](SNB%20Bridge/README.md) for the full HTTP API. |
| [SNB Desktop](SNB%20Desktop/) | Upcoming Windows desktop client (placeholder). |

## Quick start (SNB Bridge)

```powershell
cd "SNB Bridge"
flutter pub get
flutter run
```

Build a debug APK:

```powershell
flutter build apk --debug
```

## Layout

```
Say No to Bloatware/
├── README.md          ← this file
├── SNB Bridge/        ← Flutter Android app
└── SNB Desktop/       ← Windows client (coming soon)
```
