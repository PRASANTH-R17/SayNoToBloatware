import 'package:flutter/material.dart';
import 'package:intl/intl.dart';

import '../../models/activity_log_entry.dart';
import '../../models/log_event_type.dart';
import 'section_card.dart';

class ActivityMonitorSection extends StatelessWidget {
  const ActivityMonitorSection({super.key, required this.entries});

  final List<ActivityLogEntry> entries;

  Color _colorForType(LogEventType type) {
    switch (type) {
      case LogEventType.info:
        return const Color(0xFF64B5F6);
      case LogEventType.request:
        return const Color(0xFFFFCA28);
      case LogEventType.response:
        return const Color(0xFF66BB6A);
      case LogEventType.warning:
        return const Color(0xFFFF9800);
      case LogEventType.error:
        return const Color(0xFFEF5350);
    }
  }

  @override
  Widget build(BuildContext context) {
    return SectionCard(
      title: 'Live Activity Monitor',
      child: SizedBox(
        height: 220,
        child: entries.isEmpty
            ? const Center(
                child: Text(
                  'No activity yet',
                  style: TextStyle(
                    color: Color(0xFF666666),
                    fontFamily: 'monospace',
                  ),
                ),
              )
            : ListView.builder(
                itemCount: entries.length,
                itemBuilder: (context, index) {
                  final entry = entries[index];
                  final time =
                      DateFormat('HH:mm:ss').format(entry.timestamp);
                  final color = _colorForType(entry.type);

                  return Padding(
                    padding: const EdgeInsets.symmetric(vertical: 3),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          '[$time] ${entry.message}',
                          style: TextStyle(
                            color: color,
                            fontFamily: 'monospace',
                            fontSize: 12,
                          ),
                        ),
                        if (entry.detail != null)
                          Padding(
                            padding: const EdgeInsets.only(left: 8, top: 1),
                            child: Text(
                              entry.detail!,
                              style: const TextStyle(
                                color: Color(0xFFAAAAAA),
                                fontFamily: 'monospace',
                                fontSize: 11,
                              ),
                            ),
                          ),
                      ],
                    ),
                  );
                },
              ),
      ),
    );
  }
}
