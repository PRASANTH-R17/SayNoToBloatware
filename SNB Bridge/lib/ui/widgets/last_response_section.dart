import 'package:flutter/material.dart';
import 'package:intl/intl.dart';

import '../../models/bridge_response.dart';
import 'section_card.dart';

class LastResponseSection extends StatelessWidget {
  const LastResponseSection({super.key, required this.response});

  final BridgeResponse response;

  String _formatTimestamp(DateTime? ts) =>
      ts != null ? DateFormat('yyyy-MM-dd HH:mm:ss').format(ts) : '—';

  String _formatSize(int? bytes) {
    if (bytes == null) return '—';
    if (bytes < 1024) return '$bytes B';
    if (bytes < 1024 * 1024) {
      return '${(bytes / 1024).toStringAsFixed(2)} KB';
    }
    return '${(bytes / (1024 * 1024)).toStringAsFixed(2)} MB';
  }

  @override
  Widget build(BuildContext context) {
    return SectionCard(
      title: 'Last Response',
      child: Column(
        children: [
          InfoRow(label: 'Timestamp', value: _formatTimestamp(response.timestamp)),
          InfoRow(label: 'Status', value: response.status ?? '—'),
          if (response.applicationsReturned != null)
            InfoRow(
              label: 'Applications',
              value: '${response.applicationsReturned}',
            ),
          InfoRow(
            label: 'Response Size',
            value: _formatSize(response.responseSizeBytes),
          ),
          InfoRow(
            label: 'Processing Time',
            value: response.processingTimeSeconds != null
                ? '${response.processingTimeSeconds!.toStringAsFixed(3)}s'
                : '—',
          ),
        ],
      ),
    );
  }
}
