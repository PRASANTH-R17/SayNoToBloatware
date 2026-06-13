class ServerInfo {
  const ServerInfo({
    required this.status,
    required this.port,
    this.startTime,
  });

  final String status;
  final int port;
  final DateTime? startTime;

  ServerInfo copyWith({
    String? status,
    int? port,
    DateTime? startTime,
  }) {
    return ServerInfo(
      status: status ?? this.status,
      port: port ?? this.port,
      startTime: startTime ?? this.startTime,
    );
  }
}
