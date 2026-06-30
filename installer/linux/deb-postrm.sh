#!/bin/sh
set -e

# Remove per-user cache (icons, snb.db) and preferences created by standard package installs.
# Portable AppImage builds have no uninstall hook and intentionally leave user data in place.
# RPM passes 0 on final uninstall; Debian passes remove|purge.
case "$1" in
    remove|purge|0)
        if [ -d /home ]; then
            for homedir in /home/*; do
                [ -d "$homedir" ] || continue
                rm -rf "$homedir/.local/share/SayNoToBloatware"
            done
        fi

        if [ -d /root/.local/share/SayNoToBloatware ]; then
            rm -rf /root/.local/share/SayNoToBloatware
        fi
        ;;
esac

exit 0
