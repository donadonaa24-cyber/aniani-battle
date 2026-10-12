using System.Reflection;
using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine.InputSystem;

namespace AniBato.Tests
{
    internal static class InputFixtureFocus
    {
        private static object Runtime(InputTestFixture fixture) => typeof(InputTestFixture).GetProperty("runtime", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(fixture);
        public static int ReportGamepad(InputTestFixture fixture, UnityEngine.InputSystem.Layouts.InputDeviceDescription description, int deviceId = 0)
        {
            object runtime = Runtime(fixture);
            var method = runtime.GetType().GetMethods().Single(m => m.Name == "ReportNewInputDevice" &&
                m.GetParameters().Length == 5 && m.GetParameters()[0].ParameterType == typeof(UnityEngine.InputSystem.Layouts.InputDeviceDescription));
            return (int)method.Invoke(runtime, new object[] { description, deviceId, 0ul, null, null });
        }
        public static void ReportRemoved(InputTestFixture fixture, InputDevice device)
        {
            object runtime = Runtime(fixture);
            runtime.GetType().GetMethod("ReportInputDeviceRemoved", new[] { typeof(InputDevice) }).Invoke(runtime, new object[] { device });
        }
        // This package keeps runtime and FocusFlags internal. Reflection is confined to test setup.
        public static void Configure(InputTestFixture fixture)
        {
            var runtimeProperty = typeof(InputTestFixture).GetProperty("runtime", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(runtimeProperty, Is.Not.Null, "InputTestFixture.runtime API changed.");
            object runtime = runtimeProperty.GetValue(fixture);
            Assert.That(runtime, Is.Not.Null);
            var type = runtime.GetType();
            var background = type.GetProperty("runInBackground"); var focus = type.GetProperty("focusState");
            Assert.That(background, Is.Not.Null); Assert.That(focus, Is.Not.Null);
            background.SetValue(runtime, true);
            focus.SetValue(runtime, Enum.Parse(focus.PropertyType, "ApplicationFocus"));
            fixture.ScheduleFocusChangedEvent(true);
            Assert.That((bool)background.GetValue(runtime), Is.True);
            Assert.That(type.GetProperty("isPlayerFocused").GetValue(runtime), Is.EqualTo(true));
        }
    }
}
