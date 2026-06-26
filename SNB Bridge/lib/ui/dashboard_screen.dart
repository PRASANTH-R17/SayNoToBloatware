import 'package:flutter/material.dart';

import '../services/bridge_monitor.dart';
import 'widgets/activity_monitor_section.dart';
import 'widgets/connection_status_section.dart';
import 'widgets/device_info_section.dart';
import 'widgets/error_monitor_section.dart';
import 'widgets/last_request_section.dart';
import 'widgets/last_response_section.dart';
import 'widgets/server_info_section.dart';
import 'widgets/statistics_section.dart';

class DashboardScreen extends StatelessWidget {
  const DashboardScreen({super.key, required this.monitor});

  final BridgeMonitor monitor;

  @override
  Widget build(BuildContext context) {
    return ListenableBuilder(
      listenable: monitor,
      builder: (context, _) {
        return Scaffold(
          backgroundColor: const Color(0xFF121212),
          appBar: AppBar(
            title: const Text(
              'SNB Bridge',
              style: TextStyle(
                fontFamily: 'monospace',
                fontWeight: FontWeight.bold,
              ),
            ),
            backgroundColor: const Color(0xFF1A1A1A),
            foregroundColor: const Color(0xFF00BCD4),
            elevation: 0,
            automaticallyImplyLeading: false,
          ),
          body: ListView(
            padding: const EdgeInsets.symmetric(vertical: 8),
            children: [
              ConnectionStatusSection(status: monitor.connectionStatus),
              ServerInfoSection(serverInfo: monitor.serverInfo),
              DeviceInfoSection(deviceInfo: monitor.deviceInfo),
              ActivityMonitorSection(entries: monitor.activityLog),
              LastRequestSection(request: monitor.lastRequest),
              LastResponseSection(response: monitor.lastResponse),
              ErrorMonitorSection(error: monitor.lastError),
              StatisticsSection(statistics: monitor.statistics),
              const SizedBox(height: 16),
            ],
          ),
        );
      },
    );
  }
}
