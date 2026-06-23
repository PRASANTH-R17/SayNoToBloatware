import '../models/activity_log_entry.dart';
import '../models/log_event_type.dart';

class ActivityLog {
  ActivityLog({this.maxEntries = 500});

  final int maxEntries;
  final List<ActivityLogEntry> _entries = [];

  List<ActivityLogEntry> get entries => List.unmodifiable(_entries);

  void add({
    required LogEventType type,
    required String message,
    String? detail,
    DateTime? timestamp,
  }) {
    _entries.insert(
      0,
      ActivityLogEntry(
        timestamp: timestamp ?? DateTime.now(),
        type: type,
        message: message,
        detail: detail,
      ),
    );
    if (_entries.length > maxEntries) {
      _entries.removeRange(maxEntries, _entries.length);
    }
  }

  void clear() {
    _entries.clear();
  }
}
