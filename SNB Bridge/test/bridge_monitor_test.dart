import 'package:flutter_test/flutter_test.dart';

import 'package:snb_bridge/models/connection_status.dart';
import 'package:snb_bridge/models/log_event_type.dart';
import 'package:snb_bridge/services/activity_log.dart';
import 'package:snb_bridge/services/bridge_monitor.dart';

void main() {
  group('ActivityLog', () {
    test('inserts newest entries first', () {
      final log = ActivityLog(maxEntries: 10);
      log.add(type: LogEventType.info, message: 'first');
      log.add(type: LogEventType.request, message: 'second');

      expect(log.entries.length, 2);
      expect(log.entries.first.message, 'second');
      expect(log.entries.last.message, 'first');
    });

    test('trims to max cap', () {
      final log = ActivityLog(maxEntries: 3);
      for (var i = 0; i < 5; i++) {
        log.add(type: LogEventType.info, message: 'entry-$i');
      }

      expect(log.entries.length, 3);
      expect(log.entries.first.message, 'entry-4');
      expect(log.entries.last.message, 'entry-2');
    });

    test('stores event types and detail', () {
      final log = ActivityLog();
      log.add(
        type: LogEventType.request,
        message: 'Request Received',
        detail: 'GET /apps',
      );

      final entry = log.entries.first;
      expect(entry.type, LogEventType.request);
      expect(entry.detail, 'GET /apps');
    });
  });

  group('BridgeMonitor', () {
    test('transitions connection status on server lifecycle', () {
      final monitor = BridgeMonitor();

      monitor.onServerStarted(port: 5000, startTime: DateTime(2026, 6, 10));
      expect(monitor.connectionStatus.label, 'Waiting for Connection');
      expect(monitor.serverInfo.status, 'Running');

      monitor.onServerStopped();
      expect(monitor.connectionStatus.label, 'Disconnected');
      expect(monitor.serverInfo.status, 'Stopped');
    });

    test('transitions status on request and response', () {
      final monitor = BridgeMonitor();
      monitor.onServerStarted(port: 5000, startTime: DateTime.now());

      monitor.onRequestReceived(
        method: 'GET',
        endpoint: '/health',
        clientIp: '127.0.0.1',
      );
      expect(monitor.connectionStatus.label, 'Request Processing');
      expect(monitor.lastRequest.method, 'GET');
      expect(monitor.lastRequest.endpoint, '/health');

      monitor.onResponseSent(
        status: 'SUCCESS',
        responseSizeBytes: 15,
        processingTimeSeconds: 0.012,
      );
      expect(monitor.connectionStatus.label, 'Connected');
      expect(monitor.lastResponse.status, 'SUCCESS');
    });

    test('records errors and logs activity', () {
      final monitor = BridgeMonitor();

      monitor.onError(
        code: 'PACKAGE_MANAGER_ERROR',
        message: 'Failed to load apps',
      );

      expect(monitor.lastError.code, 'PACKAGE_MANAGER_ERROR');
      expect(monitor.activityLog.first.type, LogEventType.error);
    });
  });
}
