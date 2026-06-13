import 'dart:convert';

import 'package:flutter/services.dart';

import '../models/app_statistics.dart';
import 'bridge_monitor.dart';

class PackageManagerService {
  PackageManagerService({MethodChannel? channel})
      : _channel = channel ??
            const MethodChannel('com.prasanth.snb.bridge/package_manager');

  final MethodChannel _channel;

  Future<List<Map<String, dynamic>>> getInstalledApps() async {
    final result = await _channel.invokeMethod<List<dynamic>>('getInstalledApps');
    if (result == null) return [];
    return result
        .map((e) => Map<String, dynamic>.from(e as Map))
        .toList();
  }

  Future<List<Map<String, dynamic>>> getInstalledAppsWithIcons() async {
    final result =
        await _channel.invokeMethod<List<dynamic>>('getInstalledAppsWithIcons');
    if (result == null) return [];
    return result
        .map((e) => Map<String, dynamic>.from(e as Map))
        .toList();
  }

  Future<List<Map<String, dynamic>>> getAppsWithIconsByPackageNames(
    List<String> packageNames,
  ) async {
    final result = await _channel.invokeMethod<List<dynamic>>(
      'getAppsWithIconsByPackageNames',
      {'packageNames': packageNames},
    );
    if (result == null) return [];
    return result
        .map((e) => Map<String, dynamic>.from(e as Map))
        .toList();
  }

  Future<Uint8List?> getAppIcon(String packageName) async {
    final result = await _channel.invokeMethod<Uint8List>(
      'getAppIcon',
      {'packageName': packageName},
    );
    return result;
  }

  Future<AppStatistics> getAppStatistics() async {
    final result = await _channel.invokeMethod<Map<dynamic, dynamic>>(
      'getAppStatistics',
    );
    if (result == null) return const AppStatistics();
    return AppStatistics(
      totalApps: result['totalApps'] as int? ?? 0,
      userApps: result['userApps'] as int? ?? 0,
      systemApps: result['systemApps'] as int? ?? 0,
      enabledApps: result['enabledApps'] as int? ?? 0,
      disabledApps: result['disabledApps'] as int? ?? 0,
      launcherApps: result['launcherApps'] as int? ?? 0,
      serviceApps: result['serviceApps'] as int? ?? 0,
    );
  }

  Future<void> loadStatisticsInto(BridgeMonitor monitor) async {
    try {
      final stats = await getAppStatistics();
      monitor.setStatistics(stats);
    } catch (e) {
      monitor.onError(
        code: 'PACKAGE_MANAGER_ERROR',
        message: 'Failed to load app statistics',
        logDetail: e.toString(),
      );
    }
  }
}

String encodeAppsJson(List<Map<String, dynamic>> apps) {
  return jsonEncode(apps);
}
