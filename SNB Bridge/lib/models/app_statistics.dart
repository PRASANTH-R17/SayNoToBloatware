class AppStatistics {
  const AppStatistics({
    this.totalApps = 0,
    this.userApps = 0,
    this.systemApps = 0,
    this.enabledApps = 0,
    this.disabledApps = 0,
    this.launcherApps = 0,
    this.serviceApps = 0,
  });

  final int totalApps;
  final int userApps;
  final int systemApps;
  final int enabledApps;
  final int disabledApps;
  final int launcherApps;
  final int serviceApps;

  AppStatistics copyWith({
    int? totalApps,
    int? userApps,
    int? systemApps,
    int? enabledApps,
    int? disabledApps,
    int? launcherApps,
    int? serviceApps,
  }) {
    return AppStatistics(
      totalApps: totalApps ?? this.totalApps,
      userApps: userApps ?? this.userApps,
      systemApps: systemApps ?? this.systemApps,
      enabledApps: enabledApps ?? this.enabledApps,
      disabledApps: disabledApps ?? this.disabledApps,
      launcherApps: launcherApps ?? this.launcherApps,
      serviceApps: serviceApps ?? this.serviceApps,
    );
  }
}
