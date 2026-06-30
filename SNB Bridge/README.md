# SNB Bridge

SNB Bridge is the Android app for the **Say No to Bloatware** debloater ecosystem. It runs an HTTP server on the device and exposes installed-app data to the desktop client over ADB port forwarding.

**Default port:** `5000`  
**Base URL (from your PC):** `http://127.0.0.1:5000`

---

## Prerequisites

1. SNB Bridge is **open and running** on the Android device or emulator.
2. ADB port forwarding is configured:

```powershell
adb forward tcp:5000 tcp:5000
```

> Use `adb forward`, not `adb reverse`. The HTTP server runs on the device; your PC connects through the forward tunnel.

3. Verify connectivity:

```powershell
curl.exe http://127.0.0.1:5000/health
```

---

## API Overview

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/health` | Server health check |
| `GET` | `/apps` | All installed apps (metadata only) |
| `GET` | `/apps/full` | All installed apps with base64 icons |
| `POST` | `/apps/query` | Selected apps with base64 icons |
| `GET` | `/icon/{packageName}` | Single app icon (PNG binary) |

All endpoints are read-only. No authentication is required.

---

## Shared App Object

Endpoints that return app data use this JSON object shape.

### Metadata only (`GET /apps`)

```json
{
  "packageName": "com.google.android.youtube",
  "label": "YouTube",
  "isSystem": false,
  "enabled": true,
  "versionName": "19.47.39",
  "sizeBytes": 45678901,
  "permissions": ["android.permission.INTERNET"]
}
```

### With icon (`GET /apps/full`, `POST /apps/query`)

```json
{
  "packageName": "com.google.android.youtube",
  "label": "YouTube",
  "isSystem": false,
  "enabled": true,
  "iconBase64": "iVBORw0KGgoAAAANSUhEUgAA..."
}
```

| Field | Type | Description |
|-------|------|-------------|
| `packageName` | string | Android package identifier |
| `label` | string | Human-readable app name |
| `isSystem` | boolean | `true` if a system app |
| `enabled` | boolean | `true` if the app is enabled |
| `versionName` | string | App version name from `PackageManager` (empty string if unavailable) |
| `sizeBytes` | number | Combined APK size in bytes |
| `permissions` | string[] | Requested permissions declared by the app |
| `iconBase64` | string \| null | Full-resolution PNG, Base64-encoded (`NO_WRAP`). `null` if the icon cannot be loaded. Present on `/apps/full` and `/apps/query` only. |

### Decoding icons

**PowerShell:**

```powershell
[IO.File]::WriteAllBytes('icon.png', [Convert]::FromBase64String($app.iconBase64))
```

**C#:**

```csharp
var bytes = Convert.FromBase64String(app.IconBase64);
```

Use [`scripts/Export-AppIcons.ps1`](scripts/Export-AppIcons.ps1) to export all icons from a saved JSON file.

---

## Endpoints

### `GET /health`

Lightweight ping to confirm the server is running.

**Request:** No body.

**Response:** `200 OK` — `application/json`

```json
{"status":"ok"}
```

**Example:**

```powershell
curl.exe http://127.0.0.1:5000/health
```

---

### `GET /apps`

Returns all installed applications (metadata only, no icons). Also refreshes dashboard statistics on the device.

**Request:** No body.

**Response:** `200 OK` — `application/json`

```json
[
  {
    "packageName": "com.google.android.youtube",
    "label": "YouTube",
    "isSystem": false,
    "enabled": true
  }
]
```

**Example:**

```powershell
curl.exe http://127.0.0.1:5000/apps
```

---

### `GET /apps/full`

Returns all installed applications with metadata and base64-encoded icons in a single response.

**Request:** No body.

**Response:** `200 OK` — `application/json` — array of app objects with `iconBase64`.

**Notes:**

- Response can be large (several MB) on devices with many apps.
- First request may take several seconds while icons are encoded.

**Example:**

```powershell
curl.exe http://127.0.0.1:5000/apps/full -o apps-full.json
```

---

### `POST /apps/query`

Returns metadata and icons for a specific list of package names. Packages not installed on the device are omitted.

**Request:** `application/json`

```json
{
  "packageNames": [
    "com.google.android.youtube",
    "com.android.settings",
    "com.not.installed"
  ]
}
```

| Rule | Behavior |
|------|----------|
| Duplicate names | De-duplicated |
| Blank entries | Skipped |
| Non-string entries | Skipped |
| Not installed | Omitted from response |
| Empty `packageNames` | `400 Bad Request` |

**Response:** `200 OK` — `application/json` — array of app objects with `iconBase64` (same shape as `/apps/full`).

```json
[
  {
    "packageName": "com.google.android.youtube",
    "label": "YouTube",
    "isSystem": false,
    "enabled": true,
    "iconBase64": "iVBORw0KGgo..."
  },
  {
    "packageName": "com.android.settings",
    "label": "Settings",
    "isSystem": true,
    "enabled": true,
    "iconBase64": "iVBORw0KGgo..."
  }
]
```

**Error responses:**

| Status | Body | Cause |
|--------|------|-------|
| `400` | `{"error":"..."}` | Invalid JSON, missing `packageNames`, `packageNames` not an array, or empty list after validation |
| `500` | `{"error":"Internal server error"}` | Package manager or server failure |

**Example:**

```powershell
curl.exe -X POST http://127.0.0.1:5000/apps/query `
  -H "Content-Type: application/json" `
  -d "{\"packageNames\":[\"com.google.android.youtube\",\"com.android.settings\"]}"
```

---

### `GET /icon/{packageName}`

Returns a single app icon as raw PNG bytes.

**Path parameter:** `packageName` — e.g. `com.google.android.youtube`

**Response (success):** `200 OK` — `image/png` (binary)

**Response (not found):** `404 Not Found` — `application/json`

```json
{"error":"Icon not found for com.example.missing"}
```

**Example:**

```powershell
curl.exe http://127.0.0.1:5000/icon/com.google.android.youtube -o youtube.png
```

---

## Error Responses (all endpoints)

| Status | Body | When |
|--------|------|------|
| `400` | `{"error":"..."}` | Invalid request body (`POST /apps/query` only) |
| `404` | `{"error":"..."}` | Icon not found (`GET /icon/...`) |
| `500` | `{"error":"Internal server error"}` | Unhandled server or package manager error |

---

## Connection and Dashboard

The SNB Bridge UI is a read-only monitoring dashboard. It updates automatically when requests are received.

| Dashboard status | Meaning |
|------------------|---------|
| Waiting for Connection | Server running; no HTTP request received yet |
| Request Processing | Handling a request |
| Connected | At least one request completed successfully |
| Disconnected | Server stopped or failed to start |

Setting up `adb forward` alone does not change the status. Send a request (e.g. `GET /health`) to move to **Connected**.

---

## Running the App

From the repo root:

```bash
cd "SNB Bridge"
flutter pub get
flutter run
```

Install an existing debug build:

```bash
flutter build apk --debug
adb install build/app/outputs/flutter-apk/app-debug.apk
```

---

## Testing

```bash
flutter test
```

Export icons from a saved `/apps/full` or `/apps/query` JSON file (PowerShell):

```powershell
.\scripts\Export-AppIcons.ps1
```

Input and output paths are configured inside the script.

---

## Recommended Client Workflow

1. `GET /health` — verify the bridge is reachable.
2. `GET /apps` — fetch the app list (metadata only, fast).
3. `POST /apps/query` — fetch icons only for apps you need to display.
4. Alternatively, `GET /apps/full` — fetch everything in one call if you need all icons at once.

For a single icon, use `GET /icon/{packageName}` directly.
