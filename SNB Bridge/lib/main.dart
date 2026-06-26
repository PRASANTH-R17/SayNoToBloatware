import 'package:flutter/material.dart';

import 'services/bridge_monitor.dart';
import 'services/bridge_server.dart';
import 'services/device_info_service.dart';
import 'ui/dashboard_screen.dart';

void main() async {
  WidgetsFlutterBinding.ensureInitialized();

  final monitor = BridgeMonitor();
  final deviceInfoService = DeviceInfoService();
  await deviceInfoService.loadInto(monitor);

  final server = BridgeServer(monitor: monitor, port: 5000);
  await server.start();

  runApp(BridgeApp(monitor: monitor));
}

class BridgeApp extends StatelessWidget {
  const BridgeApp({super.key, required this.monitor});

  final BridgeMonitor monitor;

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'SNB Bridge',
      debugShowCheckedModeBanner: false,
      theme: ThemeData(
        brightness: Brightness.dark,
        scaffoldBackgroundColor: const Color(0xFF121212),
        colorScheme: ColorScheme.fromSeed(
          seedColor: const Color(0xFF00BCD4),
          brightness: Brightness.dark,
        ),
        fontFamily: 'monospace',
        useMaterial3: true,
      ),
      home: DashboardScreen(monitor: monitor),
    );
  }
}
