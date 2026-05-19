# OverlayForge Architecture

## Overview

OverlayForge follows **Clean Architecture** principles, organized into focused layers with clear dependency rules:

```
UI Layer (OverlayForge.UI)
    ↓
Application/Engine Layer (OverlayForge.OverlayEngine, OverlayForge.Hotkeys)
    ↓
Infrastructure Layer (OverlayForge.Infrastructure, OverlayForge.Configuration, OverlayForge.Win32)
    ↓
Core Layer (OverlayForge.Core)
```

Dependencies only flow **downward**. The Core layer has no dependencies on any other project.

---

## Project Descriptions

### OverlayForge.Core
**Target:** `net8.0` (cross-platform)

Pure domain layer. Contains:
- **Models:** `OverlayModel`, `HotkeyBinding`, `OverlayPreset`, `AppSettings`
- **Interfaces:** `IOverlayManager`, `IHotkeyService`, `IPresetService`, `ISettingsService`, `IImageService`, `IFileSystem`
- **Enums:** `OverlayState`, `HotkeyModifiers`, `HotkeyAction`, `ImageFormat`, `SnapTarget`

No Win32 or WPF dependencies. All cross-platform testable.

### OverlayForge.Configuration
**Target:** `net8.0` (cross-platform)

JSON persistence layer:
- `SettingsService` — reads/writes `settings.json`, provides defaults and migration
- `PresetService` — manages overlay preset files in `%AppData%\OverlayForge\presets\`
- Uses `System.Text.Json` with `JsonStringEnumConverter` for human-readable files

### OverlayForge.Infrastructure
**Target:** `net8.0` (cross-platform)

Infrastructure services:
- `FileSystemService` — `IFileSystem` implementation using `System.IO`
- `ImageService` — validates image files, reads dimensions from binary headers
- `LoggingConfiguration` — Serilog setup (console + rolling file sink)

### OverlayForge.Win32
**Target:** `net8.0-windows`

Windows API declarations:
- `NativeMethods` — P/Invoke for `user32.dll`, `dwmapi.dll` (WS_EX_LAYERED, RegisterHotKey, etc.)
- `MonitorHelper` — Multi-monitor enumeration via `EnumDisplayMonitors`
- `WindowStyleHelper` — Extension methods for WPF windows (overlay styles, click-through, opacity, dark mode)

All P/Invoke uses `LibraryImport` (source-generated, AOT-compatible).

### OverlayForge.OverlayEngine
**Target:** `net8.0-windows` (uses WPF)

Core overlay rendering:
- `OverlayWindow` — borderless WPF `Window` that renders an image with full alpha transparency. Implements drag, resize, and lock behaviors
- `OverlayManager` — creates/manages/destroys overlay windows, implements `IOverlayManager`

Window configuration:
- `WS_EX_LAYERED` — enables per-pixel alpha transparency
- `WS_EX_TOOLWINDOW` — removes from taskbar/Alt+Tab
- `WS_EX_NOACTIVATE` — prevents stealing game focus
- `WS_EX_TRANSPARENT` — click-through mode (when enabled)

### OverlayForge.Hotkeys
**Target:** `net8.0-windows`

Global hotkey management:
- `HotkeyService` — creates a hidden `HwndSource` message window, registers hotkeys with `RegisterHotKey`, handles `WM_HOTKEY` messages
- Uses `MOD_NOREPEAT` to prevent key repeat floods
- ID namespace: `0x7000 + (int)HotkeyAction` to avoid conflicts

### OverlayForge.UI
**Target:** `net8.0-windows` (WPF WinExe)

Presentation layer:
- **MVVM** using CommunityToolkit.Mvvm (`ObservableObject`, `RelayCommand`, `ObservableProperty`)
- `MainViewModel` — coordinates overlays, hotkeys, presets
- `OverlayListViewModel` — observable collection of overlay items
- `OverlayItemViewModel` — per-overlay state (opacity slider, lock/visibility toggles)
- `SettingsViewModel` — hotkey editor and general settings
- `MainWindow` + `SettingsWindow` — WPF views with XAML bindings
- `DarkTheme.xaml` — complete dark theme resource dictionary
- `App.xaml.cs` — DI container setup, startup/shutdown lifecycle

---

## Dependency Injection

The app uses `Microsoft.Extensions.DependencyInjection`:

```csharp
// Core infrastructure
services.AddSingleton<IFileSystem, FileSystemService>();
services.AddSingleton<IImageService, ImageService>();

// Configuration
services.AddSingleton<ISettingsService, SettingsService>();
services.AddSingleton<IPresetService, PresetService>();

// Engine
services.AddSingleton<IOverlayManager, OverlayManager>();
services.AddSingleton<IHotkeyService, HotkeyService>();

// ViewModels & Views
services.AddSingleton<MainViewModel>();
services.AddSingleton<OverlayListViewModel>();
services.AddSingleton<SettingsViewModel>();
services.AddSingleton<MainWindow>();
```

---

## Data Flow

### Adding an Overlay
```
User clicks "+ Add Image"
  → OverlayListViewModel.AddOverlayFromFileCommand
    → OpenFileDialog
      → IOverlayManager.CreateOverlay(path)
        → new OverlayModel created
        → new OverlayWindow created + shown
          → Win32: MakeOverlayWindow() applies WS_EX_LAYERED|WS_EX_TOOLWINDOW
          → Win32: SetWindowOpacity() applies alpha
        → OverlayChanged event fired
          → OverlayListViewModel adds OverlayItemViewModel to collection
            → UI updates via binding
```

### Hotkey Press
```
Global hotkey (e.g. Ctrl+Alt+H) pressed
  → Win32: WM_HOTKEY message sent to HotkeyService message window
    → HotkeyService.HotkeyTriggered event
      → MainViewModel.OnHotkeyTriggered
        → IOverlayManager.ToggleVisibility(activeId)
          → OverlayWindow visibility updated
          → OverlayChanged event → UI updates
```

---

## Configuration Storage

Files stored in `%AppData%\OverlayForge\`:

```
%AppData%\OverlayForge\
  settings.json          — AppSettings (hotkeys, preferences)
  presets\
    {guid}.json          — OverlayPreset files
  logs\
    overlayforge-YYYYMMDD.log
```

All JSON uses `System.Text.Json` with `WriteIndented = true` for human readability and `JsonStringEnumConverter` for readable enum values.

---

## Win32 Window Style Reference

| Style | Purpose |
|---|---|
| `WS_EX_LAYERED` | Enable per-pixel transparency and `SetLayeredWindowAttributes` |
| `WS_EX_TRANSPARENT` | Click-through: all mouse events pass to the window below |
| `WS_EX_TOOLWINDOW` | Hide from taskbar and Alt+Tab switcher |
| `WS_EX_NOACTIVATE` | Prevent window from stealing keyboard focus from games |
| `HWND_TOPMOST` | Keep window above all others via `SetWindowPos` |

Transparency is set via `SetLayeredWindowAttributes(hwnd, 0, alpha, LWA_ALPHA)` where `alpha` is 0-255.

---

## Testing Strategy

Cross-platform tests (run on any OS):
- `OverlayForge.Core.Tests` — model validation, enum flags, cloning
- `OverlayForge.Configuration.Tests` — settings load/save/migrate, preset CRUD
- `OverlayForge.Infrastructure.Tests` — image validation, PNG/BMP header parsing

Windows-only (CI runs on `windows-latest`):
- Full solution build including WPF projects
- Manual integration testing for overlay rendering, hotkeys

Mocking: `Moq` for `IFileSystem`, assertions via `FluentAssertions`.
