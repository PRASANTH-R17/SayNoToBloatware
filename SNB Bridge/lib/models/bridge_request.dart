class BridgeRequest {
  const BridgeRequest({
    this.timestamp,
    this.method,
    this.endpoint,
    this.clientIp,
  });

  final DateTime? timestamp;
  final String? method;
  final String? endpoint;
  final String? clientIp;

  static const empty = BridgeRequest();
}
