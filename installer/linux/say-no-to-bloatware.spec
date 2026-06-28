Name:           say-no-to-bloatware
Version:        %{version}
Release:        1%{?dist}
Summary:        Desktop debloater for Android over ADB
License:        MIT
URL:            https://github.com/PRASANTH-R17/SayNoToBloatware
BuildArch:      x86_64
Requires:       glibc >= 2.17, libgcc, libstdc++, zlib
Recommends:     libicu
AutoReqProv:    no

%description
Say No to Bloatware connects to an Android device over USB (ADB) and helps
you safely identify and remove or disable pre-installed bloatware. ADB and
the on-device companion app are bundled; no root is required.

%install
rm -rf %{buildroot}
mkdir -p %{buildroot}/opt/say-no-to-bloatware
cp -a %{_sourcedir}/payload/. %{buildroot}/opt/say-no-to-bloatware/
chmod +x %{buildroot}/opt/say-no-to-bloatware/SNB.Desktop
chmod +x %{buildroot}/opt/say-no-to-bloatware/adb 2>/dev/null || true

mkdir -p %{buildroot}/usr/bin
cat > %{buildroot}/usr/bin/say-no-to-bloatware << 'LAUNCHEREOF'
#!/bin/sh
exec /opt/say-no-to-bloatware/SNB.Desktop "$@"
LAUNCHEREOF
chmod +x %{buildroot}/usr/bin/say-no-to-bloatware

mkdir -p %{buildroot}/usr/share/applications
cat > %{buildroot}/usr/share/applications/say-no-to-bloatware.desktop << 'DESKTOPEOF'
[Desktop Entry]
Type=Application
Name=Say No to Bloatware
GenericName=Android Debloater
Comment=Find and remove pre-installed Android bloatware over USB, no root required
Exec=say-no-to-bloatware
Icon=say-no-to-bloatware
Terminal=false
Categories=Utility;System;
Keywords=android;adb;bloatware;debloat;uninstall;
DESKTOPEOF

mkdir -p %{buildroot}/usr/share/icons/hicolor/256x256/apps
cp %{_sourcedir}/app-icon.png %{buildroot}/usr/share/icons/hicolor/256x256/apps/say-no-to-bloatware.png

mkdir -p %{buildroot}/lib/udev/rules.d
cp %{_sourcedir}/51-android-usb.rules %{buildroot}/lib/udev/rules.d/70-say-no-to-bloatware-android.rules
chmod 644 %{buildroot}/lib/udev/rules.d/70-say-no-to-bloatware-android.rules

%files
/opt/say-no-to-bloatware
/usr/bin/say-no-to-bloatware
/usr/share/applications/say-no-to-bloatware.desktop
/usr/share/icons/hicolor/256x256/apps/say-no-to-bloatware.png
/lib/udev/rules.d/70-say-no-to-bloatware-android.rules

%post
%include %{_specdir}/deb-postinst.sh

%postun
%include %{_specdir}/deb-postrm.sh
