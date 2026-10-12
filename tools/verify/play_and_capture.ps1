# Launch a Windows build, send real key presses, and save window screenshots.
# Usage:
#   powershell -ExecutionPolicy Bypass -File tools\verify\play_and_capture.ps1 -Tag art2 `
#     -Steps "shot:start; hold:D:700; tap:J; wait:300; shot:after_jab; hold:L:300; shot:guard"
# Step syntax (separated by ';'):
#   shot:<name>          save docs\screenshots\<Tag>_<name>.png
#   hold:<KEY>:<ms>      press KEY, wait ms, release
#   down:<KEY> / up:<KEY>
#   tap:<KEY>            press and release (60 ms)
#   wait:<ms>
# KEY: a letter/digit (A, D, J, K, L, U, I, R ...), or SPACE, SHIFT, LEFT, RIGHT, UP, DOWN, ENTER,
#      DELETE, END, HOME, PAGEUP, PAGEDOWN, RCTRL, F1..F12
# Notes: needs a real desktop session (not the Codex sandbox). The game window must stay in front.
#        ASCII only on purpose (Windows PowerShell 5.1).
param(
    [Parameter(Mandatory = $true)][string]$Tag,
    [string]$Exe = '',
    [Parameter(Mandatory = $true)][string]$Steps,
    [int]$StartupSeconds = 6
)
$ErrorActionPreference = 'Stop'
$Root = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
if (-not $Exe) { $Exe = Join-Path $Root "Build\$Tag\AniBato.exe" }
$Out = Join-Path $Root 'docs\screenshots'
New-Item -ItemType Directory -Force $Out | Out-Null
Add-Type -AssemblyName System.Drawing
Add-Type @"
using System; using System.Runtime.InteropServices;
public class AniBatoInput {
  [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr h, out R r);
  [DllImport("user32.dll")] public static extern bool GetClientRect(IntPtr h, out R r);
  [DllImport("user32.dll")] public static extern bool ClientToScreen(IntPtr h, ref P p);
  [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr h);
  [DllImport("user32.dll")] public static extern void keybd_event(byte vk, byte scan, uint flags, UIntPtr extra);
  [DllImport("user32.dll")] public static extern uint MapVirtualKey(uint code, uint type);
  [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
  [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr h, int cmd);
  public struct R { public int L, T, Ri, B; }
  public struct P { public int X, Y; }
  public static void Down(byte vk, bool ext) { keybd_event(vk, (byte)MapVirtualKey(vk, 0), ext ? 1u : 0u, UIntPtr.Zero); }
  public static void Up(byte vk, bool ext) { keybd_event(vk, (byte)MapVirtualKey(vk, 0), (ext ? 1u : 0u) | 2u, UIntPtr.Zero); }
}
"@
$named = @{ SPACE = 0x20; SHIFT = 0xA0; LEFT = 0x25; UP = 0x26; RIGHT = 0x27; DOWN = 0x28; ENTER = 0x0D; DELETE = 0x2E;
    END = 0x23; HOME = 0x24; PAGEUP = 0x21; PAGEDOWN = 0x22; RCTRL = 0xA3; ESC = 0x1B }
$extended = @('LEFT', 'UP', 'RIGHT', 'DOWN', 'DELETE', 'END', 'HOME', 'PAGEUP', 'PAGEDOWN', 'RCTRL')
function Get-Key([string]$k) {
    $k = $k.Trim().ToUpper()
    if ($named.ContainsKey($k)) { return @([byte]$named[$k], ($extended -contains $k)) }
    if ($k -match '^F([0-9]{1,2})$') { return @([byte](0x6F + [int]$Matches[1]), $false) }
    if ($k.Length -eq 1) { return @([byte][char]$k, $false) }
    throw "Unknown key: $k"
}
$proc = Start-Process $Exe -ArgumentList '-screen-fullscreen 0 -screen-width 1280 -screen-height 720' -PassThru
Start-Sleep $StartupSeconds; $proc.Refresh()
# SAFETY: key events go to whatever window is in front. Never send a key unless the game window is the
# foreground window; otherwise keystrokes would be typed into the user's other apps.
function Test-GameInFront { return [AniBatoInput]::GetForegroundWindow() -eq $proc.MainWindowHandle }
[AniBatoInput]::ShowWindow($proc.MainWindowHandle, 9) | Out-Null
[AniBatoInput]::SetForegroundWindow($proc.MainWindowHandle) | Out-Null; Start-Sleep -Milliseconds 800
if (-not (Test-GameInFront)) {
    Stop-Process $proc -Force
    Write-Host 'ABORT: Windows did not bring the game window to the front (someone is using the PC). No keys were sent.'
    Write-Host 'Run this only when nobody is typing, or click the game window yourself and use manual checks.'
    exit 4
}
function Save-Shot([string]$name) {
    if (-not (Test-GameInFront)) { throw 'Refusing to capture another foreground app.' }
    $r = New-Object AniBatoInput+R; $origin = New-Object AniBatoInput+P
    if (-not [AniBatoInput]::GetClientRect($proc.MainWindowHandle, [ref]$r) -or
        -not [AniBatoInput]::ClientToScreen($proc.MainWindowHandle, [ref]$origin)) { throw 'Cannot resolve game client bounds.' }
    $bmp = New-Object Drawing.Bitmap ($r.Ri - $r.L), ($r.B - $r.T)
    $g = [Drawing.Graphics]::FromImage($bmp); $g.CopyFromScreen($origin.X, $origin.Y, 0, 0, $bmp.Size)
    $path = Join-Path $Out "${Tag}_$name.png"; $bmp.Save($path); $g.Dispose(); $bmp.Dispose(); Write-Host "saved $path"
}
try {
    foreach ($step in ($Steps -split ';')) {
        $parts = $step.Trim() -split ':'
        if (-not $parts[0]) { continue }
        if ($parts[0].ToLower() -in 'down', 'tap', 'hold' -and -not (Test-GameInFront)) {
            Write-Host "ABORT at '$step': the game window lost focus. Remaining keys were NOT sent."; break
        }
        switch ($parts[0].ToLower()) {
            'shot' { Save-Shot $parts[1] }
            'wait' { Start-Sleep -Milliseconds ([int]$parts[1]) }
            'down' { $k = Get-Key $parts[1]; [AniBatoInput]::Down($k[0], $k[1]) }
            'up' { $k = Get-Key $parts[1]; [AniBatoInput]::Up($k[0], $k[1]) }
            'tap' { $k = Get-Key $parts[1]; [AniBatoInput]::Down($k[0], $k[1]); Start-Sleep -Milliseconds 60; [AniBatoInput]::Up($k[0], $k[1]) }
            'hold' { $k = Get-Key $parts[1]; [AniBatoInput]::Down($k[0], $k[1]); Start-Sleep -Milliseconds ([int]$parts[2]); [AniBatoInput]::Up($k[0], $k[1]) }
            default { throw "Unknown step: $step" }
        }
    }
}
finally { if (-not $proc.HasExited) { Stop-Process $proc -Force } }
