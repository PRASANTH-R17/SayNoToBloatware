import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';

import 'package:snb_bridge/models/app_statistics.dart';
import 'package:snb_bridge/models/bridge_device_info.dart';
import 'package:snb_bridge/models/log_event_type.dart';
import 'package:snb_bridge/services/bridge_monitor.dart';
import 'package:snb_bridge/ui/dashboard_screen.dart';

void main() {
  testWidgets('DashboardScreen renders all sections with seeded state',
      (WidgetTester tester) async {
    tester.view.physicalSize = const Size(1080, 4000);
    tester.view.devicePixelRatio = 1.0;
    addTearDown(tester.view.resetPhysicalSize);

    final monitor = BridgeMonitor();
    monitor.onServerStarted(port: 5000, startTime: DateTime(2026, 6, 10, 12, 0, 0));
    monitor.setDeviceInfo(
      const BridgeDeviceInfo(
        manufacturer: 'Google',
        model: 'Pixel 8',
        androidVersion: '14',
        sdkInt: 34,
      ),
    );
    monitor.setStatistics(
      const AppStatistics(
        totalApps: 412,
        userApps: 120,
        systemApps: 292,
        enabledApps: 400,
        disabledApps: 12,
        launcherApps: 45,
        serviceApps: 80,
      ),
    );
    monitor.log(type: LogEventType.info, message: 'HTTP Server Started');
    monitor.onRequestReceived(
      method: 'GET',
      endpoint: '/apps',
      clientIp: '127.0.0.1',
    );
    monitor.onResponseSent(
      status: 'SUCCESS',
      applicationsReturned: 412,
      responseSizeBytes: 1024,
      processingTimeSeconds: 0.5,
    );

    await tester.pumpWidget(
      MaterialApp(
        home: DashboardScreen(monitor: monitor),
      ),
    );

    expect(find.text('SNB Bridge'), findsOneWidget);
    expect(find.text('Connection Status'), findsOneWidget);
    expect(find.text('Server Information'), findsOneWidget);
    expect(find.text('Device Information'), findsOneWidget);
    expect(find.text('Live Activity Monitor'), findsOneWidget);
    expect(find.text('Last Request'), findsOneWidget);
    expect(find.text('Last Response'), findsOneWidget);
    expect(find.text('Error Monitor'), findsOneWidget);
    expect(find.text('Statistics'), findsOneWidget);

    expect(find.text('Connected'), findsOneWidget);
    expect(find.text('Google'), findsOneWidget);
    expect(find.text('Pixel 8'), findsOneWidget);
    expect(find.text('412'), findsWidgets);
    expect(find.text('GET /apps'), findsOneWidget);
    expect(find.text('No errors'), findsOneWidget);
  });
}
