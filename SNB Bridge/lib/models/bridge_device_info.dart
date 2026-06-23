class BridgeDeviceInfo {
  const BridgeDeviceInfo({
    required this.manufacturer,
    required this.model,
    required this.androidVersion,
    required this.sdkInt,
  });

  final String manufacturer;
  final String model;
  final String androidVersion;
  final int sdkInt;

  static const empty = BridgeDeviceInfo(
    manufacturer: '—',
    model: '—',
    androidVersion: '—',
    sdkInt: 0,
  );
}
