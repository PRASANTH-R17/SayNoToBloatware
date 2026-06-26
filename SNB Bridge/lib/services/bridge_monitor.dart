import 'package:flutter/foundation.dart';

import '../models/activity_log_entry.dart';
import '../models/app_statistics.dart';
import '../models/bridge_device_info.dart';
import '../models/bridge_error.dart';
import '../models/bridge_request.dart';
import '../models/bridge_response.dart';
import '../models/connection_status.dart';
import '../models/log_event_type.dart';
import '../models/server_info.dart';
import 'activity_log.dart';

class BridgeMonitor extends ChangeNotifier {
  BridgeMonitor({ActivityLog? activityLog})
      : _activityLog = activityLog ?? ActivityLog();

  final ActivityLog _activityLog;

  ConnectionStatus _connectionStatus = ConnectionStatus.disconnected;
  ServerInfo _serverInfo = const ServerInfo(status: 'Stopped', port: 5000);
  BridgeDeviceInfo _deviceInfo = BridgeDeviceInfo.empty;
  BridgeRequest _lastRequest = BridgeRequest.empty;
  BridgeResponse _lastResponse = BridgeResponse.empty;
  BridgeError _lastError = BridgeError.empty;
  AppStatistics _statistics = const AppStatistics();
  bool _hasReceivedRequest = false;

  ConnectionStatus get connectionStatus => _connectionStatus;
  ServerInfo get serverInfo => _serverInfo;
  BridgeDeviceInfo get deviceInfo => _deviceInfo;
  List<ActivityLogEntry> get activityLog => _activityLog.entries;
  BridgeRequest get lastRequest => _lastRequest;
  BridgeResponse get lastResponse => _lastResponse;
  BridgeError get lastError => _lastError;
  AppStatistics get statistics => _statistics;

  void setConnectionStatus(ConnectionStatus status) {
    if (_connectionStatus == status) return;
    _connectionStatus = status;
    notifyListeners();
  }

  void setServerInfo(ServerInfo info) {
    _serverInfo = info;
    notifyListeners();
  }

  void setDeviceInfo(BridgeDeviceInfo info) {
    _deviceInfo = info;
    notifyListeners();
  }

  void setStatistics(AppStatistics stats) {
    _statistics = stats;
    notifyListeners();
  }

  void log({
    required LogEventType type,
    required String message,
    String? detail,
    DateTime? timestamp,
  }) {
    _activityLog.add(
      type: type,
      message: message,
      detail: detail,
      timestamp: timestamp,
    );
    notifyListeners();
  }

  void onServerStarted({required int port, required DateTime startTime}) {
    _hasReceivedRequest = false;
    _connectionStatus = ConnectionStatus.waitingForConnection;
    _serverInfo = ServerInfo(status: 'Running', port: port, startTime: startTime);
    log(type: LogEventType.info, message: 'HTTP Server Started');
    notifyListeners();
  }

  void onServerStopped() {
    _connectionStatus = ConnectionStatus.disconnected;
    _serverInfo = _serverInfo.copyWith(status: 'Stopped');
    log(type: LogEventType.warning, message: 'HTTP Server Stopped');
    notifyListeners();
  }

  void onServerStartFailed(String message) {
    _connectionStatus = ConnectionStatus.disconnected;
    _serverInfo = _serverInfo.copyWith(status: 'Failed');
    _lastError = BridgeError(
      timestamp: DateTime.now(),
      code: 'SERVER_START_ERROR',
      message: message,
    );
    log(type: LogEventType.error, message: 'HTTP Server Failed to Start', detail: message);
    notifyListeners();
  }

  void onRequestReceived({
    required String method,
    required String endpoint,
    required String clientIp,
  }) {
    final now = DateTime.now();
    _lastRequest = BridgeRequest(
      timestamp: now,
      method: method,
      endpoint: endpoint,
      clientIp: clientIp,
    );
    _hasReceivedRequest = true;
    _connectionStatus = ConnectionStatus.requestProcessing;
    log(
      type: LogEventType.request,
      message: 'Request Received',
      detail: '$method $endpoint',
      timestamp: now,
    );
    notifyListeners();
  }

  void onResponseSent({
    required String status,
    int? applicationsReturned,
    int? responseSizeBytes,
    required double processingTimeSeconds,
  }) {
    final now = DateTime.now();
    _lastResponse = BridgeResponse(
      timestamp: now,
      status: status,
      applicationsReturned: applicationsReturned,
      responseSizeBytes: responseSizeBytes,
      processingTimeSeconds: processingTimeSeconds,
    );
    _connectionStatus = _hasReceivedRequest
        ? ConnectionStatus.connected
        : ConnectionStatus.waitingForConnection;

    final details = <String>[
      'Status: $status',
      if (applicationsReturned != null) 'Apps: $applicationsReturned',
      if (responseSizeBytes != null) 'Size: $responseSizeBytes bytes',
      'Time: ${processingTimeSeconds.toStringAsFixed(3)}s',
    ];
    log(
      type: LogEventType.response,
      message: 'Response Sent',
      detail: details.join(' | '),
      timestamp: now,
    );
    notifyListeners();
  }

  void onError({
    required String code,
    required String message,
    String? logDetail,
  }) {
    final now = DateTime.now();
    _lastError = BridgeError(
      timestamp: now,
      code: code,
      message: message,
    );
    _connectionStatus = _hasReceivedRequest
        ? ConnectionStatus.connected
        : ConnectionStatus.waitingForConnection;
    log(
      type: LogEventType.error,
      message: message,
      detail: logDetail ?? code,
      timestamp: now,
    );
    notifyListeners();
  }
}
