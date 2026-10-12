using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

namespace AniBato.Tests
{
    // USB report 0x01, as parsed by Input System's DualSenseGamepadHID.
    // Complete reports exercise the real HID parser; delta events are deliberately rejected by it.
    [StructLayout(LayoutKind.Explicit, Size = 64)]
    internal struct DualSenseUsbReport : IInputStateTypeInfo
    {
        public FourCC format => new FourCC('H', 'I', 'D');
        [FieldOffset(0)] public byte ReportId;
        [FieldOffset(1)] public byte LeftX;
        [FieldOffset(2)] public byte LeftY;
        [FieldOffset(3)] public byte RightX;
        [FieldOffset(4)] public byte RightY;
        [FieldOffset(5)] public byte LeftTrigger;
        [FieldOffset(6)] public byte RightTrigger;
        [FieldOffset(8)] public byte Buttons0;
        [FieldOffset(9)] public byte Buttons1;
        [FieldOffset(10)] public byte Buttons2;

        public static DualSenseUsbReport From(Gamepad pad, ISet<ButtonControl> held)
        {
            var report = new DualSenseUsbReport { ReportId = 1, LeftX = 128, LeftY = 128, RightX = 128, RightY = 128, Buttons0 = 8 };
            if (held.Contains(pad.buttonWest)) report.Buttons0 |= 16;
            if (held.Contains(pad.buttonSouth)) report.Buttons0 |= 32;
            if (held.Contains(pad.buttonEast)) report.Buttons0 |= 64;
            if (held.Contains(pad.buttonNorth)) report.Buttons0 |= 128;
            if (held.Contains(pad.leftShoulder)) report.Buttons1 |= 1;
            if (held.Contains(pad.rightShoulder)) report.Buttons1 |= 2;
            if (held.Contains(pad.selectButton)) report.Buttons1 |= 16;
            if (held.Contains(pad.startButton)) report.Buttons1 |= 32;
            if (held.Contains(pad.leftStickButton)) report.Buttons1 |= 64;
            if (held.Contains(pad.rightStickButton)) report.Buttons1 |= 128;
            if (held.Contains(pad.leftTrigger)) report.LeftTrigger = 255;
            if (held.Contains(pad.rightTrigger)) report.RightTrigger = 255;
            return report;
        }
    }
}
