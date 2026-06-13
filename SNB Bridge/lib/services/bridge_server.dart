import 'dart:convert';
import 'dart:io';

import 'package:shelf/shelf.dart';
import 'package:shelf/shelf_io.dart' as shelf_io;
import 'package:shelf_router/shelf_router.dart';

import 'apps_query_parser.dart';
import 'bridge_monitor.dart';
import 'package_manager_service.dart';

class BridgeServer {
  BridgeServer({
    required this.monitor,
    this.port = 5000,
    PackageManagerService? packageManager,
  })  : _packageManager = packageManager ?? PackageManagerService();

  final BridgeMonitor monitor;
  final int port;
  final PackageManagerService _packageManager;

  HttpServer? _server;

  Future<void> start() async {
    if (_server != null) return;

    final router = Router();

    router.get('/health', (Request request) async {
      return _handleRequest(request, () async {
        const body = '{"status":"ok"}';
        return Response.ok(
          body,
          headers: {'Content-Type': 'application/json'},
          context: {'bodyBytes': body.length},
        );
      });
    });

    router.get('/apps', (Request request) async {
      return _handleRequest(request, () async {
        final apps = await _packageManager.getInstalledApps();
        final body = encodeAppsJson(apps);
        final stats = await _packageManager.getAppStatistics();
        monitor.setStatistics(stats);
        return Response.ok(
          body,
          headers: {'Content-Type': 'application/json'},
          context: {
            'applicationsReturned': apps.length,
            'bodyBytes': body.length,
          },
        );
      });
    });

    router.get('/apps/full', (Request request) async {
      return _handleRequest(request, () async {
        final apps = await _packageManager.getInstalledAppsWithIcons();
        final body = encodeAppsJson(apps);
        final stats = await _packageManager.getAppStatistics();
        monitor.setStatistics(stats);
        return Response.ok(
          body,
          headers: {'Content-Type': 'application/json'},
          context: {
            'applicationsReturned': apps.length,
            'bodyBytes': body.length,
          },
        );
      });
    });

    router.post('/apps/query', (Request request) async {
      return _handleRequest(request, () async {
        List<String> packageNames;
        try {
          final body = await request.readAsString();
          packageNames = parsePackageNamesRequest(body);
        } on FormatException catch (e) {
          final errorBody = jsonEncode({'error': e.message});
          return Response(
            400,
            body: errorBody,
            headers: {'Content-Type': 'application/json'},
            context: {'bodyBytes': errorBody.length},
          );
        }

        if (packageNames.isEmpty) {
          const errorBody = '{"error":"packageNames must not be empty"}';
          return Response(
            400,
            body: errorBody,
            headers: {'Content-Type': 'application/json'},
            context: {'bodyBytes': errorBody.length},
          );
        }

        final apps =
            await _packageManager.getAppsWithIconsByPackageNames(packageNames);
        final body = encodeAppsJson(apps);
        return Response.ok(
          body,
          headers: {'Content-Type': 'application/json'},
          context: {
            'applicationsReturned': apps.length,
            'bodyBytes': body.length,
          },
        );
      });
    });

    router.get('/icon/<packageName>', (Request request, String packageName) async {
      return _handleRequest(request, () async {
        final iconBytes = await _packageManager.getAppIcon(packageName);
        if (iconBytes == null || iconBytes.isEmpty) {
          final body = '{"error":"Icon not found for $packageName"}';
          return Response.notFound(
            body,
            headers: {'Content-Type': 'application/json'},
            context: {'bodyBytes': body.length},
          );
        }
        return Response.ok(
          iconBytes,
          headers: {'Content-Type': 'image/png'},
          context: {'bodyBytes': iconBytes.length},
        );
      });
    });

    final handler = Pipeline().addHandler(router.call);

    try {
      _server = await shelf_io.serve(handler, InternetAddress.anyIPv4, port);
      monitor.onServerStarted(port: port, startTime: DateTime.now());
      await _packageManager.loadStatisticsInto(monitor);
    } catch (e) {
      monitor.onServerStartFailed(e.toString());
    }
  }

  Future<void> stop() async {
    await _server?.close(force: true);
    _server = null;
    monitor.onServerStopped();
  }

  Future<Response> _handleRequest(
    Request request,
    Future<Response> Function() handler,
  ) async {
    final stopwatch = Stopwatch()..start();
    final method = request.method;
    final endpoint = request.requestedUri.path;
    final clientIp = _clientIp(request);

    monitor.onRequestReceived(
      method: method,
      endpoint: endpoint,
      clientIp: clientIp,
    );

    try {
      final response = await handler();
      stopwatch.stop();

      final bodyBytes = response.context['bodyBytes'] as int? ?? 0;
      final applicationsReturned =
          response.context['applicationsReturned'] as int?;

      final statusLabel =
          response.statusCode >= 200 && response.statusCode < 300
              ? 'SUCCESS'
              : 'ERROR';

      monitor.onResponseSent(
        status: statusLabel,
        applicationsReturned: applicationsReturned,
        responseSizeBytes: bodyBytes,
        processingTimeSeconds: stopwatch.elapsedMilliseconds / 1000.0,
      );

      return response;
    } catch (e) {
      stopwatch.stop();
      monitor.onError(
        code: 'PACKAGE_MANAGER_ERROR',
        message: e.toString(),
      );
      monitor.onResponseSent(
        status: 'ERROR',
        responseSizeBytes: 0,
        processingTimeSeconds: stopwatch.elapsedMilliseconds / 1000.0,
      );
      const body = '{"error":"Internal server error"}';
      return Response.internalServerError(
        body: body,
        headers: {'Content-Type': 'application/json'},
        context: {'bodyBytes': body.length},
      );
    }
  }

  String _clientIp(Request request) {
    final httpRequest = request.context['shelf.io.request'];
    if (httpRequest is HttpRequest) {
      return httpRequest.connectionInfo?.remoteAddress.address ?? 'unknown';
    }
    return 'unknown';
  }
}
