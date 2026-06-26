import 'package:flutter/material.dart';

import '../../models/connection_status.dart';
import 'section_card.dart';

class ConnectionStatusSection extends StatelessWidget {
  const ConnectionStatusSection({super.key, required this.status});

  final ConnectionStatus status;

  Color get _dotColor {
    switch (status) {
      case ConnectionStatus.waitingForConnection:
        return const Color(0xFFFFC107);
      case ConnectionStatus.connected:
        return const Color(0xFF4CAF50);
      case ConnectionStatus.requestProcessing:
        return const Color(0xFF2196F3);
      case ConnectionStatus.disconnected:
        return const Color(0xFFF44336);
    }
  }

  @override
  Widget build(BuildContext context) {
    return SectionCard(
      title: 'Connection Status',
      child: Row(
        children: [
          Container(
            width: 12,
            height: 12,
            decoration: BoxDecoration(
              color: _dotColor,
              shape: BoxShape.circle,
              boxShadow: [
                BoxShadow(
                  color: _dotColor.withValues(alpha: 0.6),
                  blurRadius: 6,
                  spreadRadius: 1,
                ),
              ],
            ),
          ),
          const SizedBox(width: 10),
          Text(
            status.label,
            style: const TextStyle(
              color: Color(0xFFE0E0E0),
              fontSize: 15,
              fontWeight: FontWeight.w600,
              fontFamily: 'monospace',
            ),
          ),
        ],
      ),
    );
  }
}
