class BridgeError {
  const BridgeError({
    this.timestamp,
    this.code,
    this.message,
  });

  final DateTime? timestamp;
  final String? code;
  final String? message;

  static const empty = BridgeError();
}
