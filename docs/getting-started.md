# Getting Started

This guide gets you from a fresh download to your first device scan.

## 1. System requirements

**Desktop (your computer):**

- Windows 10/11 (x64) or a modern Linux distribution (x64).
- A USB cable to connect your phone.
- That's it — **ADB is bundled** with the app, so you do not need to install Android platform-tools separately.

**Android device:**

- Android phone or tablet (Android 5.0+ recommended).
- **USB debugging** enabled (steps below).

## 2. Enable USB debugging on your phone

USB debugging lets the desktop app talk to your device over ADB.

1. Open **Settings → About phone**.
2. Tap **Build number** seven times until you see "You are now a developer".
3. Go back to **Settings → System → Developer options** (location varies by brand).
4. Turn on **USB debugging**.
5. Connect the phone to your computer with a USB cable.
6. On the phone, a dialog titled **"Allow USB debugging?"** will appear. Check
   **"Always allow from this computer"** and tap **Allow**.

> The authorization prompt is required. If you miss it or tap "Deny", the desktop app will not be
> able to see the device. Unplug and replug, or revoke USB debugging authorizations from Developer
> options and reconnect to make it reappear.

### Where is Developer options on my brand?

| Brand | Path to Build number |
|-------|----------------------|
| Stock Android / Pixel | Settings → About phone → Build number |
| Samsung | Settings → About phone → Software information → Build number |
| Xiaomi / Redmi / POCO | Settings → About phone → MIUI/HyperOS version |
| Vivo / iQOO | Settings → About phone → Software version (tap version repeatedly) |
| Oppo / Realme | Settings → About device → Version |

## 3. Get the app

- **Download a release** build (recommended for most users), or
- **Build from source** — see [Build from Source](build-from-source.md).

No installation of ADB or the on-device bridge is needed: the desktop app carries a bundled `adb`
and automatically installs/launches the **SNB Bridge** companion app on your device the first time
it connects.

## 4. First run and first scan

1. Launch **SNB Desktop**.
2. On the **Select a Device** screen, your connected phone appears as a card (with a matched photo).
   If it is empty, press **Refresh** and confirm USB debugging is authorized.
3. Click your device to connect. On the first connection SNB will:
   - set up an ADB port-forward,
   - install and start the SNB Bridge app on the device (you may briefly see it appear),
   - scan installed apps and match them against the bloatware database.
4. You land on the **Applications** screen, where apps are grouped into **All Apps**,
   **Recommended Removal**, and **Apps with Alternatives**.

You are ready to review and remove apps — continue to the [User Guide](user-guide.md).

> First connect is the slowest because the bridge APK is installed and icons are fetched. Subsequent
> scans reuse the already-installed bridge and cached icons, so they are much faster.
