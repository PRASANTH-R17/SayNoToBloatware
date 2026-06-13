import 'dart:convert';

import 'package:flutter/services.dart';
import 'package:flutter_test/flutter_test.dart';

import 'package:snb_bridge/services/apps_query_parser.dart';
import 'package:snb_bridge/services/package_manager_service.dart';

void main() {
  TestWidgetsFlutterBinding.ensureInitialized();

  group('encodeAppsJson', () {
    test('includes iconBase64 when present', () {
      final apps = [
        {
          'packageName': 'com.google.android.youtube',
          'label': 'YouTube',
          'isSystem': false,
          'enabled': true,
          'iconBase64': 'iVBORw0KGgoAAAANSUhEUgAA',
        },
      ];

      final json = encodeAppsJson(apps);
      final decoded = jsonDecode(json) as List<dynamic>;
      final app = decoded.first as Map<String, dynamic>;

      expect(app['packageName'], 'com.google.android.youtube');
      expect(app['iconBase64'], 'iVBORw0KGgoAAAANSUhEUgAA');
    });

    test('includes null iconBase64 when icon unavailable', () {
      final apps = [
        {
          'packageName': 'com.example.broken',
          'label': 'Broken App',
          'isSystem': false,
          'enabled': true,
          'iconBase64': null,
        },
      ];

      final json = encodeAppsJson(apps);
      final decoded = jsonDecode(json) as List<dynamic>;
      final app = decoded.first as Map<String, dynamic>;

      expect(app['iconBase64'], isNull);
    });
  });

  group('parsePackageNamesRequest', () {
    test('parses valid package names and de-duplicates', () {
      const body = '''
{
  "packageNames": [
    "com.google.android.youtube",
    "com.android.settings",
    "com.google.android.youtube",
    "  "
  ]
}
''';

      final names = parsePackageNamesRequest(body);

      expect(names, [
        'com.google.android.youtube',
        'com.android.settings',
      ]);
    });

    test('returns empty list when all entries are blank or non-strings', () {
      const body = '{"packageNames": ["", "  ", 123, null]}';

      final names = parsePackageNamesRequest(body);

      expect(names, isEmpty);
    });

    test('throws when JSON is invalid', () {
      expect(
        () => parsePackageNamesRequest('not json'),
        throwsA(isA<FormatException>()),
      );
    });

    test('throws when packageNames is missing', () {
      expect(
        () => parsePackageNamesRequest('{"other": []}'),
        throwsA(
          predicate<FormatException>(
            (e) => e.message == 'packageNames is required',
          ),
        ),
      );
    });

    test('throws when packageNames is not an array', () {
      expect(
        () => parsePackageNamesRequest('{"packageNames": "com.test"}'),
        throwsA(
          predicate<FormatException>(
            (e) => e.message == 'packageNames must be a JSON array',
          ),
        ),
      );
    });

    test('throws when body is not a JSON object', () {
      expect(
        () => parsePackageNamesRequest('["com.test"]'),
        throwsA(
          predicate<FormatException>(
            (e) => e.message == 'Expected JSON object',
          ),
        ),
      );
    });
  });

  group('getAppsWithIconsByPackageNames', () {
    test('parses filtered channel response with iconBase64', () async {
      const channel = MethodChannel('com.prasanth.snb.bridge/package_manager');
      final binding = TestDefaultBinaryMessengerBinding.instance;

      binding.defaultBinaryMessenger.setMockMethodCallHandler(
        channel,
        (call) async {
          if (call.method == 'getAppsWithIconsByPackageNames') {
            expect(call.arguments, {
              'packageNames': [
                'com.google.android.youtube',
                'com.not.installed',
              ],
            });
            return [
              {
                'packageName': 'com.google.android.youtube',
                'label': 'YouTube',
                'isSystem': false,
                'enabled': true,
                'iconBase64': 'abc123',
              },
            ];
          }
          return null;
        },
      );

      final service = PackageManagerService(channel: channel);
      final apps = await service.getAppsWithIconsByPackageNames([
        'com.google.android.youtube',
        'com.not.installed',
      ]);

      expect(apps.length, 1);
      expect(apps.first['packageName'], 'com.google.android.youtube');
      expect(apps.first['iconBase64'], 'abc123');

      binding.defaultBinaryMessenger
          .setMockMethodCallHandler(channel, null);
    });
  });

  group('getInstalledAppsWithIcons', () {
    test('parses channel response with iconBase64', () async {
      const channel = MethodChannel('com.prasanth.snb.bridge/package_manager');
      final binding = TestDefaultBinaryMessengerBinding.instance;

      binding.defaultBinaryMessenger.setMockMethodCallHandler(
        channel,
        (call) async {
          if (call.method == 'getInstalledAppsWithIcons') {
            return [
              {
                'packageName': 'com.test.app',
                'label': 'Test App',
                'isSystem': true,
                'enabled': true,
                'iconBase64': 'abc123',
              },
            ];
          }
          return null;
        },
      );

      final service = PackageManagerService(channel: channel);
      final apps = await service.getInstalledAppsWithIcons();

      expect(apps.length, 1);
      expect(apps.first['packageName'], 'com.test.app');
      expect(apps.first['iconBase64'], 'abc123');

      binding.defaultBinaryMessenger
          .setMockMethodCallHandler(channel, null);
    });
  });
}
