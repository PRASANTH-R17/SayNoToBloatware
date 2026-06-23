import 'dart:convert';

/// Parses a POST /apps/query JSON body into a de-duplicated list of package names.
///
/// Throws [FormatException] when the body is invalid JSON, missing [packageNames],
/// or [packageNames] is not a JSON array.
List<String> parsePackageNamesRequest(String body) {
  final dynamic decoded;
  try {
    decoded = jsonDecode(body);
  } on FormatException {
    rethrow;
  }

  if (decoded is! Map) {
    throw const FormatException('Expected JSON object');
  }

  final packageNames = decoded['packageNames'];
  if (packageNames == null) {
    throw const FormatException('packageNames is required');
  }
  if (packageNames is! List) {
    throw const FormatException('packageNames must be a JSON array');
  }

  final names = <String>[];
  final seen = <String>{};
  for (final entry in packageNames) {
    if (entry is! String) continue;
    final trimmed = entry.trim();
    if (trimmed.isEmpty) continue;
    if (seen.add(trimmed)) {
      names.add(trimmed);
    }
  }

  return names;
}
