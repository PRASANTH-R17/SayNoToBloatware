import 'package:flutter/material.dart';
import 'package:intl/intl.dart';

import '../../models/bridge_error.dart';
import 'section_card.dart';

class ErrorMonitorSection extends StatelessWidget {
  const ErrorMonitorSection({super.key, required this.error});

  final BridgeError error;

  bool get _hasError =>
      error.timestamp != null && error.code != null && error.message != null;

  String _formatTimestamp(DateTime? ts) =>
      ts != null ? DateFormat('yyyy-MM-dd HH:mm:ss').format(ts) : '—';

  @override
  Widget build(BuildContext context) {
    return SectionCard(
      title: 'Error Monitor',
      child: _hasError
          ? Column(
              children: [
                InfoRow(label: 'Timestamp', value: _formatTimestamp(error.timestamp)),
                InfoRow(label: 'Code', value: error.code ?? '—'),
                InfoRow(label: 'Message', value: error.message ?? '—'),
              ],
            )
          : const Text(
              'No errors',
              style: TextStyle(
                color: Color(0xFF66BB6A),
                fontFamily: 'monospace',
                fontSize: 13,
              ),
            ),
    );
  }
}
