# Troubleshooting

Common problems and how to fix them. If your issue isn't here, check the
[Bridge HTTP API](../SNB%20Bridge/README.md) or open an issue.

## Device not detected

The **Select a Device** screen is empty or your phone never appears.

Checklist:

1. **USB debugging is on.** Re-check Developer options — see
   [Getting Started](getting-started.md#2-enable-usb-debugging-on-your-phone).
2. **Authorize the computer.** Look for the **"Allow USB debugging?"** prompt on the phone and tap
   **Allow** (tick "Always allow from this computer"). If you never see it, unplug/replug, or in
   Developer options use **Revoke USB debugging authorizations**, then reconnect.
3. **Use a data cable + direct port.** Some cables are charge-only. Plug directly into the computer,
   not through a hub.
4. **Switch USB mode.** On the phone's USB notification, choose **File transfer (MTP)** rather than
   "Charging only".
5. **Press Refresh** in SNB after each change.

Still nothing? Confirm ADB itself sees the device — see [ADB sanity check](#adb-sanity-check) below.

## Device shows as "unauthorized"

This means USB debugging is on but the computer isn't trusted yet. Unlock the phone, watch for the
authorization dialog, and tap **Allow**. If it doesn't appear, revoke authorizations (Developer
options) and replug.

## Wrong photo shown for my device

SNB matches a device photo from its brand/model. If the match is wrong or generic, the app still works
normally — only the picture is cosmetic. Newer or rare models may fall back to a brand-default image.

## Connecting/scanning is stuck or slow

- The **first** connection is always the slowest: it installs the bridge APK and downloads icons.
  Later scans are much faster.
- If it appears stuck, press **Refresh**/**Scan Again**, or disconnect and reconnect the cable.
- Make sure the screen is unlocked during the first connect so the bridge can start.

## Bridge / port-forward issues

The desktop reaches the on-device bridge over `adb forward tcp:5000`. If app data won't load:

1. Re-connect the device (this re-runs the port-forward and bridge health check).
2. Make sure no other tool is holding port 5000 on your computer.
3. Confirm the **SNB Bridge** app is installed on the phone; SNB reinstalls it automatically when
   missing or outdated.

## An app can't be removed or disabled

When you see **Failed** with a message like *"Protected by the device manufacturer — can't be removed
or disabled without root access,"* the app is locked down by the OEM.

What's happening under the hood:

- SNB tries `pm uninstall --user 0`. If the device returns `DELETE_FAILED_USER_RESTRICTED`, it then
  tries `pm disable-user --user 0` as a fallback.
- Some manufacturers (notably **Vivo/iQOO** on certain core apps such as `com.vivo.appstore`) block
  *both* uninstall and disable for the ADB shell user, returning a `SecurityException` ("no root
  permission").

This is a **device restriction, not a bug** — there is no non-root way to remove or disable those
specific apps. Options:

- Leave the app in place, or disable it manually in **Settings → Apps** if the OS allows it.
- Rooting the device would lift the restriction but is risky, voids warranties, and is **not** required
  or recommended for normal use.

Apps that fail for other reasons show their specific error and are never silently disabled.

## A removed app still shows in the list

After a successful removal the app is pruned from the list automatically. If something looks stale,
click **Scan Again** to re-read the device.

## ADB sanity check

If you have Android platform-tools installed separately, you can confirm the OS sees your phone:

```bash
adb devices
```

- `device` next to the serial — good, SNB should detect it.
- `unauthorized` — accept the prompt on the phone (see above).
- empty list — USB debugging/cable/driver issue.

SNB uses its own bundled `adb`, so you don't need platform-tools for the app itself — this is only for
diagnosis.
