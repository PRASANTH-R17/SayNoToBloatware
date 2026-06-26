import 'package:flutter/material.dart';
import 'package:intl/intl.dart';

import '../../models/bridge_request.dart';
import 'section_card.dart';

class LastRequestSection extends StatelessWidget {
  const LastRequestSection({super.key, required this.request});

  final BridgeRequest request;

  String _formatTimestamp(DateTime? ts) =>
      ts != null ? DateFormat('yyyy-MM-dd HH:mm:ss').format(ts) : '—';

  @override
  Widget build(BuildContext context) {
    return SectionCard(
      title: 'Last Request',
      child: Column(
        children: [
          InfoRow(label: 'Timestamp', value: _formatTimestamp(request.timestamp)),
          InfoRow(label: 'Method', value: request.method ?? '—'),
          InfoRow(label: 'Endpoint', value: request.endpoint ?? '—'),
          InfoRow(label: 'Client IP', value: request.clientIp ?? '—'),
        ],
      ),
    );
  }
}
