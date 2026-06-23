# SNB Desktop

The cross-platform desktop client for **Say No to Bloatware** — built with Avalonia and .NET 10.

It connects to an Android device over ADB, deploys the on-device [SNB Bridge](../SNB%20Bridge/README.md)
companion, classifies installed apps against a bloatware database, and removes (or disables) unwanted
apps with a clear confirmation → progress → summary flow. Includes app export (CSV/JSON), Light/Dark
themes, and English/Tamil localization.

## Projects

| Project | Path | Role |
|---------|------|------|
| `SNB.Desktop` | `src/SNB.Desktop` | Avalonia MVVM GUI (the main app). |
| `SNB.Backend` | `src/SNB.Backend` | Shared logic: ADB, bridge client, database, device matching, removal. |
| `SNB.Cli` | `src/SNB.Cli` | Console front-end over the same backend. |
| `SNB.Backend.Tests` | `tests/SNB.Backend.Tests` | xUnit tests. |

## Quick start

```bash
# from the repo root
dotnet build "SNB Desktop/SNB.sln"
dotnet run --project "SNB Desktop/src/SNB.Desktop/SNB.Desktop.csproj"
dotnet test "SNB Desktop/tests/SNB.Backend.Tests/SNB.Backend.Tests.csproj"
```

ADB and the bridge APK are bundled and copied next to the binary, so a published build is
self-contained.

## Documentation

See the project docs in **[../docs/](../docs/README.md)** — in particular the
[Architecture](../docs/architecture.md) and [Build from Source](../docs/build-from-source.md) pages.
