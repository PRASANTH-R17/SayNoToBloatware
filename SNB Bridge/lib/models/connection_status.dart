enum ConnectionStatus {
  waitingForConnection,
  connected,
  requestProcessing,
  disconnected,
}

extension ConnectionStatusLabel on ConnectionStatus {
  String get label {
    switch (this) {
      case ConnectionStatus.waitingForConnection:
        return 'Waiting for Connection';
      case ConnectionStatus.connected:
        return 'Connected';
      case ConnectionStatus.requestProcessing:
        return 'Request Processing';
      case ConnectionStatus.disconnected:
        return 'Disconnected';
    }
  }
}
