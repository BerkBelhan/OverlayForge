using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Microsoft.Extensions.Logging;
using OverlayForge.Core.Enums;
using OverlayForge.Core.Interfaces;
using OverlayForge.Core.Models;
using OverlayForge.Win32.Interop;

namespace OverlayForge.Hotkeys.Services;

/// <summary>
/// Manages global hotkeys using Win32 RegisterHotKey API.
/// Creates a hidden message-only window to receive WM_HOTKEY messages.
/// </summary>
public sealed class HotkeyService : IHotkeyService, IDisposable
{
    private readonly ILogger<HotkeyService> _logger;
    private readonly Dictionary<int, HotkeyBinding> _registeredHotkeys = new();
    private HwndSource? _hwndSource;
    private bool _disposed;

    // Hotkey ID base offset to avoid conflicts with other applications
    private const int HotkeyIdBase = 0x7000;

    public event EventHandler<HotkeyTriggeredEventArgs>? HotkeyTriggered;
    public IReadOnlyList<HotkeyBinding> RegisteredBindings =>
        _registeredHotkeys.Values.ToList().AsReadOnly();

    public HotkeyService(ILogger<HotkeyService> logger)
    {
        _logger = logger;
        InitializeMessageWindow();
    }

    private void InitializeMessageWindow()
    {
        // Create a hidden HwndSource to receive Windows messages
        var parameters = new HwndSourceParameters("OverlayForge.HotkeyReceiver")
        {
            Width = 0,
            Height = 0,
            PositionX = -32000,
            PositionY = -32000,
            WindowStyle = 0,
            ExtendedWindowStyle = NativeMethods.WS_EX_TOOLWINDOW,
            ParentWindow = IntPtr.Zero
        };

        _hwndSource = new HwndSource(parameters);
        _hwndSource.AddHook(WndProc);

        _logger.LogDebug("Hotkey message window created (hwnd={Hwnd})", _hwndSource.Handle);
    }

    public void RegisterHotkeys(IEnumerable<HotkeyBinding> bindings)
    {
        foreach (var binding in bindings.Where(b => b.IsEnabled))
            RegisterHotkey(binding);
    }

    public bool RegisterHotkey(HotkeyBinding binding)
    {
        if (_hwndSource is null || _disposed) return false;

        int id = HotkeyIdBase + (int)binding.Action;
        uint modifiers = ConvertModifiers(binding.Modifiers) | NativeMethods.MOD_NOREPEAT;

        // Unregister existing if present
        if (_registeredHotkeys.ContainsKey(id))
            NativeMethods.UnregisterHotKey(_hwndSource.Handle, id);

        bool success = NativeMethods.RegisterHotKey(
            _hwndSource.Handle,
            id,
            modifiers,
            (uint)binding.KeyCode);

        if (success)
        {
            _registeredHotkeys[id] = binding;
            _logger.LogDebug("Registered hotkey: {Display} -> {Action}", binding.DisplayString, binding.Action);
        }
        else
        {
            int error = Marshal.GetLastWin32Error();
            _logger.LogWarning("Failed to register hotkey {Display} (error {Error})", binding.DisplayString, error);
        }

        return success;
    }

    public bool UnregisterHotkey(HotkeyAction action)
    {
        if (_hwndSource is null || _disposed) return false;

        int id = HotkeyIdBase + (int)action;
        if (!_registeredHotkeys.ContainsKey(id)) return false;

        bool success = NativeMethods.UnregisterHotKey(_hwndSource.Handle, id);
        if (success)
        {
            _registeredHotkeys.Remove(id);
            _logger.LogDebug("Unregistered hotkey for action {Action}", action);
        }

        return success;
    }

    public void UnregisterAll()
    {
        if (_hwndSource is null || _disposed) return;

        foreach (var id in _registeredHotkeys.Keys.ToList())
            NativeMethods.UnregisterHotKey(_hwndSource.Handle, id);

        _registeredHotkeys.Clear();
        _logger.LogDebug("Unregistered all hotkeys.");
    }

    public bool IsConflicting(HotkeyModifiers modifiers, int keyCode, HotkeyAction? excludeAction = null)
    {
        return _registeredHotkeys.Values.Any(b =>
            b.Modifiers == modifiers &&
            b.KeyCode == keyCode &&
            (excludeAction is null || b.Action != excludeAction.Value));
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == NativeMethods.WM_HOTKEY)
        {
            int id = wParam.ToInt32();
            if (_registeredHotkeys.TryGetValue(id, out var binding))
            {
                _logger.LogDebug("Hotkey triggered: {Action}", binding.Action);
                HotkeyTriggered?.Invoke(this, new HotkeyTriggeredEventArgs(binding.Action, binding));
                handled = true;
            }
        }
        return IntPtr.Zero;
    }

    private static uint ConvertModifiers(HotkeyModifiers modifiers)
    {
        uint result = 0;
        if (modifiers.HasFlag(HotkeyModifiers.Alt)) result |= (uint)NativeMethods.MOD_ALT;
        if (modifiers.HasFlag(HotkeyModifiers.Control)) result |= (uint)NativeMethods.MOD_CONTROL;
        if (modifiers.HasFlag(HotkeyModifiers.Shift)) result |= (uint)NativeMethods.MOD_SHIFT;
        if (modifiers.HasFlag(HotkeyModifiers.Win)) result |= (uint)NativeMethods.MOD_WIN;
        return result;
    }

    public void Dispose()
    {
        if (_disposed) return;
        UnregisterAll();
        _hwndSource?.Dispose();
        _hwndSource = null;
        _disposed = true;
    }
}
