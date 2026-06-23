package com.prasanth.snb.bridge

import android.content.pm.ApplicationInfo
import android.content.pm.PackageManager
import android.graphics.Bitmap
import android.graphics.Canvas
import android.graphics.drawable.BitmapDrawable
import android.graphics.drawable.Drawable
import android.os.Build
import android.util.Base64
import io.flutter.plugin.common.MethodCall
import io.flutter.plugin.common.MethodChannel
import java.io.ByteArrayOutputStream

class PackageManagerBridge(
    private val activity: MainActivity,
) : MethodChannel.MethodCallHandler {

    companion object {
        const val CHANNEL_NAME = "com.prasanth.snb.bridge/package_manager"
    }

    private val packageManager: PackageManager
        get() = activity.packageManager

    override fun onMethodCall(call: MethodCall, result: MethodChannel.Result) {
        try {
            when (call.method) {
                "getInstalledApps" -> result.success(getInstalledApps())
                "getInstalledAppsWithIcons" -> result.success(getInstalledAppsWithIcons())
                "getAppsWithIconsByPackageNames" -> {
                    @Suppress("UNCHECKED_CAST")
                    val packageNames = call.argument<List<String>>("packageNames")
                    if (packageNames == null) {
                        result.error("INVALID_ARGUMENT", "packageNames is required", null)
                    } else {
                        result.success(getAppsWithIconsByPackageNames(packageNames))
                    }
                }
                "getAppIcon" -> {
                    val packageName = call.argument<String>("packageName")
                    if (packageName.isNullOrBlank()) {
                        result.error("INVALID_ARGUMENT", "packageName is required", null)
                    } else {
                        result.success(getAppIcon(packageName))
                    }
                }
                "getAppStatistics" -> result.success(getAppStatistics())
                else -> result.notImplemented()
            }
        } catch (e: Exception) {
            result.error("PACKAGE_MANAGER_ERROR", e.message, null)
        }
    }

    private fun installedApplicationFlags(): Int {
        return PackageManager.GET_META_DATA or
            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.N) {
                PackageManager.MATCH_DISABLED_COMPONENTS
            } else {
                @Suppress("DEPRECATION")
                PackageManager.GET_DISABLED_COMPONENTS
            }
    }

    private fun queryInstalledApplications(): List<ApplicationInfo> {
        @Suppress("DEPRECATION")
        return packageManager.getInstalledApplications(installedApplicationFlags())
    }

    private fun getInstalledApps(): List<Map<String, Any?>> {
        return queryInstalledApplications().map { appInfo ->
            appMetadataMap(appInfo)
        }
    }

    private fun getInstalledAppsWithIcons(): List<Map<String, Any?>> {
        return queryInstalledApplications().map { appInfo ->
            appMetadataMap(appInfo) + mapOf(
                "iconBase64" to encodeIconBase64(appInfo.packageName),
            )
        }
    }

    private fun getAppsWithIconsByPackageNames(packageNames: List<String>): List<Map<String, Any?>> {
        val apps = mutableListOf<Map<String, Any?>>()
        for (name in packageNames.distinct()) {
            if (name.isBlank()) continue
            try {
                val appInfo = packageManager.getApplicationInfo(name, installedApplicationFlags())
                apps.add(
                    appMetadataMap(appInfo) + mapOf(
                        "iconBase64" to encodeIconBase64(appInfo.packageName),
                    ),
                )
            } catch (_: PackageManager.NameNotFoundException) {
                // Omit packages not installed on the device.
            }
        }
        return apps
    }

    private fun appMetadataMap(appInfo: ApplicationInfo): Map<String, Any?> {
        return mapOf(
            "packageName" to appInfo.packageName,
            "label" to getAppLabel(appInfo),
            "isSystem" to ((appInfo.flags and ApplicationInfo.FLAG_SYSTEM) != 0),
            "enabled" to appInfo.enabled,
        )
    }

    private fun encodeIconBase64(packageName: String): String? {
        val bytes = getAppIcon(packageName) ?: return null
        return Base64.encodeToString(bytes, Base64.NO_WRAP)
    }

    private fun getAppLabel(appInfo: ApplicationInfo): String {
        return try {
            packageManager.getApplicationLabel(appInfo).toString()
        } catch (_: Exception) {
            appInfo.packageName
        }
    }

    private fun getAppIcon(packageName: String): ByteArray? {
        return try {
            val drawable = packageManager.getApplicationIcon(packageName)
            drawableToPngBytes(drawable)
        } catch (_: Exception) {
            null
        }
    }

    private fun drawableToPngBytes(drawable: Drawable): ByteArray? {
        val bitmap = when (drawable) {
            is BitmapDrawable -> {
                if (drawable.bitmap != null) {
                    drawable.bitmap
                } else {
                    Bitmap.createBitmap(
                        drawable.intrinsicWidth.coerceAtLeast(1),
                        drawable.intrinsicHeight.coerceAtLeast(1),
                        Bitmap.Config.ARGB_8888,
                    ).also { bitmap ->
                        val canvas = Canvas(bitmap)
                        drawable.setBounds(0, 0, canvas.width, canvas.height)
                        drawable.draw(canvas)
                    }
                }
            }
            else -> {
                val width = drawable.intrinsicWidth.coerceAtLeast(1)
                val height = drawable.intrinsicHeight.coerceAtLeast(1)
                Bitmap.createBitmap(width, height, Bitmap.Config.ARGB_8888).also { bitmap ->
                    val canvas = Canvas(bitmap)
                    drawable.setBounds(0, 0, canvas.width, canvas.height)
                    drawable.draw(canvas)
                }
            }
        }

        val stream = ByteArrayOutputStream()
        bitmap.compress(Bitmap.CompressFormat.PNG, 100, stream)
        return stream.toByteArray()
    }

    private fun getAppStatistics(): Map<String, Int> {
        val flags = PackageManager.GET_META_DATA or
            PackageManager.GET_SERVICES or
            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.N) {
                PackageManager.MATCH_DISABLED_COMPONENTS
            } else {
                @Suppress("DEPRECATION")
                PackageManager.GET_DISABLED_COMPONENTS
            }

        @Suppress("DEPRECATION")
        val apps = packageManager.getInstalledApplications(flags)

        var userApps = 0
        var systemApps = 0
        var enabledApps = 0
        var disabledApps = 0
        var serviceApps = 0

        val launcherPackages = getLauncherPackages()

        for (appInfo in apps) {
            val isSystem = (appInfo.flags and ApplicationInfo.FLAG_SYSTEM) != 0
            if (isSystem) {
                systemApps++
            } else {
                userApps++
            }

            if (appInfo.enabled) {
                enabledApps++
            } else {
                disabledApps++
            }

            try {
                val packageInfo = packageManager.getPackageInfo(
                    appInfo.packageName,
                    PackageManager.GET_SERVICES,
                )
                if (packageInfo.services != null && packageInfo.services!!.isNotEmpty()) {
                    serviceApps++
                }
            } catch (_: Exception) {
                // Skip packages we cannot inspect.
            }
        }

        return mapOf(
            "totalApps" to apps.size,
            "userApps" to userApps,
            "systemApps" to systemApps,
            "enabledApps" to enabledApps,
            "disabledApps" to disabledApps,
            "launcherApps" to launcherPackages.size,
            "serviceApps" to serviceApps,
        )
    }

    private fun getLauncherPackages(): Set<String> {
        val intent = android.content.Intent(android.content.Intent.ACTION_MAIN).apply {
            addCategory(android.content.Intent.CATEGORY_LAUNCHER)
        }
        val resolveInfos = packageManager.queryIntentActivities(intent, 0)
        return resolveInfos.map { it.activityInfo.packageName }.toSet()
    }
}
