import 'package:flutter/material.dart';
import 'package:intl/intl.dart';

import '../../models/server_info.dart';
import 'section_card.dart';

class ServerInfoSection extends StatelessWidget {
  const ServerInfoSection({super.key, required this.serverInfo});

  final ServerInfo serverInfo;

  @override
  Widget build(BuildContext context) {
    final startTime = serverInfo.startTime != null
        ? DateFormat('yyyy-MM-dd HH:mm:ss').format(serverInfo.startTime!)
        : '—';

    return SectionCard(
      title: 'Server Information',
      child: Column(
        children: [
          InfoRow(label: 'HTTP Status', value: serverInfo.status),
          InfoRow(label: 'Port', value: '${serverInfo.port}'),
          InfoRow(label: 'Started', value: startTime),
        ],
      ),
    );
  }
}
