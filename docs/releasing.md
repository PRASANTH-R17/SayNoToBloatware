# Releasing & Deployment

How Say No to Bloatware is packaged and distributed for Windows and Linux.

## Artifacts

| Platform | Type | File | How it's built |
|----------|------|------|----------------|
| Windows | Portable | `SNB-Portable-win-x64-<ver>.zip` | `WindowsPortable` publish profile (single-file, self-contained) + zip |
| Windows | Installer | `SNB-Setup-<ver>.exe` | Inno Setup (`installer/windows/snb.iss`) |
| Linux | Portable | `SNB-<ver>-x86_64.AppImage` | `installer/linux/build-appimage.sh` |
| Linux | Installer | `say-no-to-bloatware_<ver>_amd64.deb` | `installer/linux/build-deb.sh` |

All builds are **self-contained** (no .NET runtime required on the user's machine) and target x64. The bundled `adb`, the `snb_bridge.apk`, the seed database, and device images are shipped alongside the app, so nothing else needs installing.

## Automated releases (recommended)

A GitHub Actions workflow ([.github/workflows/release.yml](../.github/workflows/release.yml)) builds all four artifacts and attaches them to a GitHub Release.

```bash
# Tag a version and push it
git tag v1.0.0
git push origin v1.0.0
```

- The `windows` job (windows runner) produces the portable zip and the Inno Setup installer.
- The `linux` job (ubuntu runner) produces the AppImage and the `.deb`.
- On a tag push, both jobs upload their files to the Release for tag `v1.0.0`.
- You can also trigger the workflow manually ("Run workflow") to produce downloadable build artifacts without cutting a release.

The version is taken from the tag name (`v1.0.0` -> `1.0.0`) and passed to the build via `-p:Version=`.

## Building locally

Bump the version in [SNB.Desktop.csproj](../SNB%20Desktop/src/SNB.Desktop/SNB.Desktop.csproj) (`<Version>`), or pass `-p:Version=<ver>` on the command line.

### Windows (run on Windows)

```powershell
# Portable (single-file exe + assets)
dotnet publish "SNB Desktop/src/SNB.Desktop/SNB.Desktop.csproj" -p:PublishProfile=WindowsPortable
Compress-Archive -Path "SNB Desktop/publish/desktop/win-portable/*" -DestinationPath "dist/SNB-Portable-win-x64-1.0.0.zip"

# Installer (requires Inno Setup: https://jrsoftware.org/isdl.php)
iscc /DAppVersion=1.0.0 "/DSourceDir=$PWD\SNB Desktop\publish\desktop\win-portable" "/DOutputDir=$PWD\dist" installer\windows\snb.iss
```

### Linux (run on Linux or WSL)

```bash
dotnet publish "SNB Desktop/src/SNB.Desktop/SNB.Desktop.csproj" -p:PublishProfile=Linux

# Portable AppImage (needs curl + FUSE; the script uses appimagetool)
installer/linux/build-appimage.sh 1.0.0 "SNB Desktop/publish/desktop/linux" dist

# Debian package (needs dpkg-deb)
installer/linux/build-deb.sh 1.0.0 "SNB Desktop/publish/desktop/linux" dist
```

> Linux packaging (AppImage/.deb) must run on Linux. The .NET cross-publish itself works from Windows, but the packaging tools do not.

## Installing (end users)

- **Windows portable:** extract the zip, double-click `SNB.Desktop.exe`. No admin rights.
- **Windows installer:** run `SNB-Setup-<ver>.exe`, follow the wizard (Start Menu + optional desktop shortcut, uninstaller included).
- **Linux AppImage:** `chmod +x SNB-*.AppImage` then run it.
- **Linux .deb:** `sudo apt install ./say-no-to-bloatware_<ver>_amd64.deb` (installs to `/opt/say-no-to-bloatware`, adds a menu entry).

## Notes & caveats

- **Code signing:** the Windows installer/exe are unsigned, so SmartScreen may warn on first run. Add Authenticode signing later if desired.
- **libicu on Linux:** the `.deb` recommends `libicu`; on minimal systems install it (`sudo apt install libicu-dev`) if the app fails to start. The AppImage has the same runtime requirement.
- **Architectures:** only x64 is built today. arm64 (Windows/Linux) can be added with extra publish profiles + runners.
