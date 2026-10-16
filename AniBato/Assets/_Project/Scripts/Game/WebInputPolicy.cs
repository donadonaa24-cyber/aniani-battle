using UnityEngine;
using UnityEngine.InputSystem;
namespace AniBato.Game
{
    public static class WebInputPolicy
    {
        public static bool Enabled=>Application.platform==RuntimePlatform.WebGLPlayer;
        public static PlayerKeyBindings Keys(PlayerKeyBindings original,int player,bool web)
        {
            if(!web||player==0)return original;
            original.Jump=Key.Numpad0;original.Attack=Key.Numpad1;original.Special=Key.Numpad2;
            original.Guard=Key.Numpad3;original.Dodge=Key.Numpad4;original.Counter=Key.Numpad5;
            original.Ultimate=Key.Numpad6;original.Dash=Key.NumpadEnter;return original;
        }
        public static Key Pause(int player,bool web)=>web?(player==0?Key.P:Key.O):(player==0?Key.Escape:Key.Backspace);
        public static bool CanBind(Key key,bool web)
        {
            if(key==Key.Escape||key==Key.F9||key==Key.F11||key==Key.Tab)return false;
            if(!web)return true;
            return key!=Key.LeftCtrl&&key!=Key.RightCtrl&&key!=Key.LeftAlt&&key!=Key.RightAlt&&key!=Key.LeftMeta&&key!=Key.RightMeta&&
                key!=Key.F1&&key!=Key.F2&&key!=Key.F3&&key!=Key.F4&&key!=Key.F5&&key!=Key.F6&&key!=Key.F7&&key!=Key.F8&&key!=Key.F10&&key!=Key.F12&&
                key!=Key.Digit1&&key!=Key.Digit2&&key!=Key.Digit3&&key!=Key.Digit4&&key!=Key.M;
        }
        public static string Text(string value)=>!Enabled?value:value.Replace("F11 全画面","画面上の全画面ボタン").Replace("F9","4").Replace("F7","3").Replace("F6","2").Replace("F5","1").Replace("Tab","M");
    }
}
