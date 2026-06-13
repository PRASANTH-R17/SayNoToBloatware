import 'package:device_info_plus/device_info_plus.dart';
import 'package:flutter/foundation.dart';

import '../models/bridge_device_info.dart';
import '../models/log_event_type.dart';
import 'bridge_monitor.dart';

class DeviceInfoService {
  DeviceInfoService({DeviceInfoPlugin? plugin}) : _plugin = plugin ?? DeviceInfoPlugin();

  final DeviceInfoPlugin _plugin;

  Future<void> loadInto(BridgeMonitor monitor) async {
    try {
      if (defaultTargetPlatform == TargetPlatform.android) {
        final info = await _plugin.androidInfo;
        monitor.setDeviceInfo(
          BridgeDeviceInfo(
            manufacturer: info.manufacturer,
            model: info.model,
            androidVersion: info.version.release,
            sdkInt: info.version.sdkInt,
          ),
        );
      } else {
        monitor.setDeviceInfo(
          const BridgeDeviceInfo(
            manufacturer: 'N/A',
            model: 'Non-Android',
            androidVersion: 'N/A',
            sdkInt: 0,
          ),
        );
      }
    } catch (e) {
      monitor.log(
        type: LogEventType.warning,
        message: 'Failed to load device info',
        detail: e.toString(),
      );
    }
  }
}
