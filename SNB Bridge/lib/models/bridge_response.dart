class BridgeResponse {
  const BridgeResponse({
    this.timestamp,
    this.status,
    this.applicationsReturned,
    this.responseSizeBytes,
    this.processingTimeSeconds,
  });

  final DateTime? timestamp;
  final String? status;
  final int? applicationsReturned;
  final int? responseSizeBytes;
  final double? processingTimeSeconds;

  static const empty = BridgeResponse();
}
