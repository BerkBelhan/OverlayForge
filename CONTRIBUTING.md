# Contributing to OverlayForge

Thank you for your interest in contributing! This document provides guidelines for contributing to OverlayForge.

## Getting Started

1. **Fork** the repository
2. **Clone** your fork: `git clone https://github.com/YOUR-USERNAME/OverlayForge.git`
3. **Install** .NET 8 SDK from [dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/8)
4. **Open** `OverlayForge.sln` in Visual Studio 2022+ or JetBrains Rider

## Building

```bash
dotnet restore OverlayForge.sln
dotnet build OverlayForge.sln --configuration Release
```

## Running Tests

```bash
# Run all cross-platform tests (works on Linux/macOS/Windows)
dotnet test tests/OverlayForge.Core.Tests/
dotnet test tests/OverlayForge.Configuration.Tests/
dotnet test tests/OverlayForge.Infrastructure.Tests/

# Run full build + tests (Windows only, requires WPF)
dotnet test OverlayForge.sln --configuration Release
```

## Project Structure

```
/src
  OverlayForge.Core          — Core models, interfaces, enums (no external deps)
  OverlayForge.Configuration — JSON settings and preset management
  OverlayForge.Infrastructure — File I/O, image utilities, Serilog logging
  OverlayForge.Win32         — Win32 P/Invoke declarations (WS_EX_LAYERED, hotkeys)
  OverlayForge.OverlayEngine  — WPF overlay window management
  OverlayForge.Hotkeys       — Global hotkey service (RegisterHotKey)
  OverlayForge.UI            — WPF MVVM application shell and views
/tests                       — xUnit tests with Moq and FluentAssertions
/docs                        — Architecture documentation
```

## Code Style

- **C# 12** with nullable reference types enabled
- **MVVM** pattern for all UI code (CommunityToolkit.Mvvm)
- **Dependency injection** for all services
- **Async/await** for I/O operations
- Add XML doc comments to all public APIs
- Follow existing naming conventions

## Pull Request Guidelines

1. Create a branch from `main`: `git checkout -b feature/my-feature`
2. Make focused, minimal changes
3. Add or update tests for changed logic
4. Ensure all tests pass: `dotnet test`
5. Update documentation if needed
6. Submit a PR with a clear description of changes

## Reporting Issues

Use [GitHub Issues](../../issues) with:
- Windows version
- .NET version (`dotnet --version`)
- Steps to reproduce
- Expected vs actual behavior
- Relevant log files from `%AppData%\OverlayForge\logs\`

## Code of Conduct

Be respectful and constructive. This is an open-source project maintained in good faith.

## License

By contributing, you agree your contributions will be licensed under the MIT License.
