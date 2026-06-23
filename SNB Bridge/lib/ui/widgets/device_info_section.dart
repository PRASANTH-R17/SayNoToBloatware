import 'package:flutter/material.dart';

import '../../models/bridge_device_info.dart';
import 'section_card.dart';

class DeviceInfoSection extends StatelessWidget {
  const DeviceInfoSection({super.key, required this.deviceInfo});

  final BridgeDeviceInfo deviceInfo;

  @override
  Widget build(BuildContext context) {
    return SectionCard(
      title: 'Device Information',
      child: Column(
        children: [
          InfoRow(label: 'Manufacturer', value: deviceInfo.manufacturer),
          InfoRow(label: 'Model', value: deviceInfo.model),
          InfoRow(label: 'Android Version', value: deviceInfo.androidVersion),
          InfoRow(label: 'SDK', value: '${deviceInfo.sdkInt}'),
        ],
      ),
    );
  }
}
