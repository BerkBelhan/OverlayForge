# OverlayForge

> A modern, lightweight transparent image overlay system for Windows — the spiritual successor to Glass2k, built for gamers and designers.

[![CI](https://github.com/BerkBelhan/OverlayForge/actions/workflows/ci.yml/badge.svg)](https://github.com/BerkBelhan/OverlayForge/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![.NET 8](https://img.shields.io/badge/.NET-8.0-purple)](https://dotnet.microsoft.com/download/dotnet/8)

---

## What is OverlayForge?

OverlayForge lets you place **semi-transparent PNG images** above any game or application window for:

- 🎨 **Design reference** — trace, align, and position artwork over running applications
- 🎮 **Gaming guides** — keep maps, callouts, or crosshair overlays on-screen
- 🖼️ **Visual alignment** — compare designs against live applications
- 🎥 **Streaming tools** — optional OBS-visible or hidden overlays

It works especially well with borderless fullscreen games, DirectX 11/12 games, Vulkan games, and multi-monitor setups.

---

## Features

| Feature | Description |
|---|---|
| **Always-on-top overlays** | Transparent windows that stay above all applications |
| **True click-through** | Mouse input passes straight through to the game |
| **Per-overlay opacity** | 0-100% transparency per overlay, adjusted with hotkeys |
| **Multi-overlay support** | Manage unlimited overlays with a layers panel |
| **Drag & resize** | Intuitive mouse-based positioning and scaling |
| **Lock mode** | Prevent accidental movement of positioned overlays |
| **Global hotkeys** | Ctrl+Alt shortcuts work even when a game has focus |
| **Preset system** | Save and load overlay configurations per game |
| **PNG alpha channels** | Full transparency support with anti-aliased edges |
| **Transform tools** | Rotation, flip horizontal/vertical |
| **System tray** | Runs minimized without cluttering the taskbar |
| **Dark UI** | Modern minimal dark-mode interface |

---

## Default Hotkeys

| Shortcut | Action |
|---|---|
| `Ctrl+Alt+Up` | Increase active overlay opacity |
| `Ctrl+Alt+Down` | Decrease active overlay opacity |
| `Ctrl+Alt+C` | Toggle click-through mode |
| `Ctrl+Alt+H` | Toggle overlay visibility |
| `Ctrl+Alt+L` | Lock/unlock overlay position |

All hotkeys are fully customizable in **Settings → Hotkeys**.

---

## Requirements

- **Windows 10 version 1903+** or **Windows 11**
- **.NET 8.0 Runtime** (or use the self-contained portable build)
- No admin rights required
- No DLL injection, no anti-cheat interference

---

## Installation

### Option 1: Portable Build (recommended)
Download `OverlayForge-portable-win-x64.zip` from [Releases](../../releases), extract, and run `OverlayForge.exe`.

### Option 2: Build from Source

```bash
# Prerequisites: .NET 8 SDK, Visual Studio 2022 or later
git clone https://github.com/BerkBelhan/OverlayForge.git
cd OverlayForge
dotnet build OverlayForge.sln --configuration Release
```

Or open `OverlayForge.sln` in Visual Studio and press **F5**.

---

## Architecture

OverlayForge follows Clean Architecture with MVVM:

```
/src
  OverlayForge.Core          — Models, interfaces, enums (platform-neutral)
  OverlayForge.Configuration — Settings & preset persistence (JSON)
  OverlayForge.Infrastructure — File I/O, image loading, Serilog logging
  OverlayForge.Win32         — P/Invoke: window styles, hotkey APIs, DWM
  OverlayForge.OverlayEngine  — WPF overlay windows, transparency, drag
  OverlayForge.Hotkeys       — Global hotkey registration (RegisterHotKey)
  OverlayForge.UI            — WPF MVVM application, CommunityToolkit.Mvvm
/tests
  OverlayForge.Core.Tests
  OverlayForge.Configuration.Tests
  OverlayForge.Infrastructure.Tests
```

---

## Security

OverlayForge is a non-invasive overlay utility:
- ❌ Does NOT inject DLLs into games
- ❌ Does NOT read or modify game memory
- ❌ Does NOT bypass anti-cheat systems
- ✅ Uses only standard Windows overlay APIs (WS_EX_LAYERED, WS_EX_TRANSPARENT)
- ✅ Requires no elevated (admin) privileges

---

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

---

## License

MIT License — see [LICENSE](LICENSE) for details.