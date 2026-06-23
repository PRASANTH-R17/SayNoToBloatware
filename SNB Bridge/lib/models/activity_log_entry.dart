import 'log_event_type.dart';

class ActivityLogEntry {
  const ActivityLogEntry({
    required this.timestamp,
    required this.type,
    required this.message,
    this.detail,
  });

  final DateTime timestamp;
  final LogEventType type;
  final String message;
  final String? detail;
}
