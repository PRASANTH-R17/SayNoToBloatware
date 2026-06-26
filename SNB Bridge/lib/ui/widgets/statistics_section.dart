import 'package:flutter/material.dart';

import '../../models/app_statistics.dart';
import 'section_card.dart';

class StatisticsSection extends StatelessWidget {
  const StatisticsSection({super.key, required this.statistics});

  final AppStatistics statistics;

  @override
  Widget build(BuildContext context) {
    return SectionCard(
      title: 'Statistics',
      child: Column(
        children: [
          InfoRow(label: 'Total Apps', value: '${statistics.totalApps}'),
          InfoRow(label: 'User Apps', value: '${statistics.userApps}'),
          InfoRow(label: 'System Apps', value: '${statistics.systemApps}'),
          InfoRow(label: 'Enabled Apps', value: '${statistics.enabledApps}'),
          InfoRow(label: 'Disabled Apps', value: '${statistics.disabledApps}'),
          InfoRow(label: 'Launcher Apps', value: '${statistics.launcherApps}'),
          InfoRow(label: 'Service Apps', value: '${statistics.serviceApps}'),
        ],
      ),
    );
  }
}
