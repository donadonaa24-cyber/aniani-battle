using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.UI;

namespace AniBato.Game
{
    // Count contacts separately: lifting one finger must not release another held finger.
    public sealed class MobileControlButton : OnScreenButton, IPointerDownHandler, IPointerUpHandler
    {
        private readonly HashSet<int> contacts = new HashSet<int>();
        private Image paper;
        private Text label;
        public bool Held => contacts.Count != 0;
        public void Configure(Image image, Text text) { paper = image; label = text; Paint(); }
        public void SetLabel(string value) { if (label) label.text = value; }
        public new void OnPointerDown(PointerEventData data)
        {
            if (!isActiveAndEnabled || !contacts.Add(data.pointerId)) return;
            if (contacts.Count == 1) base.OnPointerDown(data);
            Paint();
        }
        public new void OnPointerUp(PointerEventData data)
        {
            if (!contacts.Remove(data.pointerId)) return;
            if (contacts.Count == 0) base.OnPointerUp(data);
            Paint();
        }
        public void ReleaseContacts()
        {
            contacts.Clear();
            if (control != null && control.device.added) SendValueToControl(0f);
            Paint();
        }
        private void Paint()
        {
            if (paper) paper.color = new Color(1f, .89f, .65f, Held ? .63f : .24f);
            if (label) label.color = new Color(1f, .97f, .87f, Held ? 1f : .82f);
        }
        protected override void OnDisable() { ReleaseContacts(); base.OnDisable(); }
    }
}
