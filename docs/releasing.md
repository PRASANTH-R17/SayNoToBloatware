# Releasing & Deployment

How Say No to Bloatware is packaged and distributed for Windows and Linux.

## Artifacts

| Platform | Type | File | How it's built |
|----------|------|------|----------------|
| Windows | Portable | `SNB-Portable-win-x64-<ver>.zip` | `WindowsPortable` publish profile (folder, self-contained) + zip |
| Windows | Single-file | `SNB-SingleFile-win-x64-<ver>.exe` | `WindowsSingleFile` publish profile (self-contained, bundled assets) |
| Windows | Installer | `SNB-Setup-<ver>.exe` | Inno Setup (`installer/windows/snb.iss`) |
| Linux | Portable | `SNB-<ver>-x86_64.AppImage` | `installer/linux/build-appimage.sh` |
| Linux | Installer (Debian/Ubuntu) | `say-no-to-bloatware_<ver>_amd64.deb` | `installer/linux/build-deb.sh` |
| Linux | Installer (Fedora/RHEL) | `say-no-to-bloatware-<ver>-1.x86_64.rpm` | `installer/linux/build-rpm.sh` |

All builds are **self-contained** (no .NET runtime required on the user's machine) and target x64. The bundled `adb`, the `snb_bridge.apk`, the seed database, and device images are included in every build (alongside the app for folder/portable formats, embedded in the single-file exe).

## Automated releases (recommended)

A GitHub Actions workflow ([.github/workflows/release.yml](../.github/workflows/release.yml)) builds all six artifacts and attaches them to a GitHub Release.

```bash
# Tag a version and push it
git tag v1.0.0
git push origin v1.0.0
```

- The `build-bridge` job builds the Flutter bridge APK and supplies it to both platform jobs.
- The `windows` job (windows runner) produces the portable zip, the single-file exe, and the Inno Setup installer.
- The `linux` job (ubuntu runner) produces the AppImage, the `.deb`, and the `.rpm`.
- On a tag push, both jobs upload their files to the Release for tag `v1.0.0`.
- You can also trigger the workflow manually ("Run workflow") to produce downloadable build artifacts without cutting a release.

The version is taken from the tag name (`v1.0.0` -> `1.0.0`) and passed to the build via `-p:Version=`.

## Building locally

Bump the version in [SNB.Desktop.csproj](../SNB%20Desktop/src/SNB.Desktop/SNB.Desktop.csproj) (`<Version>`), or pass `-p:Version=<ver>` on the command line.

### Windows (run on Windows)

```powershell
# Portable folder (self-contained exe + assets, zipped)
dotnet publish "SNB Desktop/src/SNB.Desktop/SNB.Desktop.csproj" -p:PublishProfile=WindowsPortable
Compress-Archive -Path "SNB Desktop/publish/desktop/win-portable/*" -DestinationPath "dist/SNB-Portable-win-x64-1.0.0.zip"

# Single-file (one self-contained exe; assets extract on first run)
dotnet publish "SNB Desktop/src/SNB.Desktop/SNB.Desktop.csproj" -p:PublishProfile=WindowsSingleFile
Copy-Item "SNB Desktop/publish/desktop/win-single/SNB.Desktop.exe" "dist/SNB-SingleFile-win-x64-1.0.0.exe"

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

# RPM package (needs rpmbuild from the rpm package)
installer/linux/build-rpm.sh 1.0.0 "SNB Desktop/publish/desktop/linux" dist
```

> Linux packaging (AppImage/.deb/.rpm) must run on Linux. The .NET cross-publish itself works from Windows, but the packaging tools do not.

## Installing (end users)

- **Windows portable:** extract the zip, double-click `SNB.Desktop.exe`. No admin rights. User data (icon cache, preferences) stays under `%LocalAppData%\SayNoToBloatware` and `%AppData%\SayNoToBloatware` even if you delete the extracted folder.
- **Windows single-file:** double-click `SNB-SingleFile-win-x64-<ver>.exe`. No install or zip extract. On first run, bundled assets are extracted to a cache under `%TEMP%\.net\` (override with `DOTNET_BUNDLE_EXTRACT_BASE_DIR` if needed).
- **Windows installer:** run `SNB-Setup-<ver>.exe`, follow the wizard (Start Menu + optional desktop shortcut, uninstaller included). Uninstalling removes that user data automatically.
- **Linux AppImage:** `chmod +x SNB-*.AppImage` then run it. Deleting the AppImage does not remove `~/.local/share/SayNoToBloatware`.
- **Linux .deb:** `sudo apt install ./say-no-to-bloatware_<ver>_amd64.deb` (installs to `/opt/say-no-to-bloatware`, adds a menu entry, ships udev rules). `apt remove` / `apt purge` removes per-user data under `~/.local/share/SayNoToBloatware`.
- **Linux .rpm:** `sudo dnf install ./say-no-to-bloatware_<ver>-1.x86_64.rpm` on Fedora, RHEL 8+, Rocky, or Alma (same install layout and udev rules as the `.deb`). `dnf remove` / `rpm -e` removes per-user data under `~/.local/share/SayNoToBloatware`.

## Notes & caveats

- **Code signing:** the Windows builds are unsigned, so SmartScreen may warn on first run. Add Authenticode signing later if desired.
- **Single-file extraction:** the single-file exe unpacks bundled assets to `%TEMP%\.net\` on startup. If bundled assets fail to resolve, use the portable zip or installer instead.
- **libicu on Linux:** the `.deb` and `.rpm` recommend `libicu`; on minimal systems install it if the app fails to start (e.g. `sudo apt install libicu-dev` on Debian/Ubuntu, `sudo dnf install libicu` on Fedora). The AppImage has the same runtime requirement.
- **Architectures:** only x64 is built today. arm64 (Windows/Linux) can be added with extra publish profiles + runners.
