using System.Collections.Generic;

namespace Zapper.Core.WebOs
{
    public static class WebOsActionKey
    {
        public const string Left = "Left";
        public const string Right = "Right";
        public const string Down = "Down";
        public const string Up = "Up";
        public const string Home = "Home";
        public const string Back = "Back";
        public const string Menu = "Menu";
        public const string Enter = "Enter";
        public const string Dash = "Dash";
        public const string Info = "Info";
        public const string One = "1";
        public const string Two = "2";
        public const string Three = "3";
        public const string Four = "4";
        public const string Five = "5";
        public const string Six = "6";
        public const string Seven = "7";
        public const string Eight = "8";
        public const string Nine = "9";
        public const string Zero = "0";
        public const string Asterisk = "Asterisk";
        public const string Cc = "Closed Captioning";
        public const string Exit = "Exit";
        public const string TurnOn3d = "Turn on 3D";
        public const string TurnOff3d = "Turn off 3D";
        public const string Mute = "Mute";
        public const string Unmute = "Unmute";
        public const string Red = "Red";
        public const string Green = "Green";
        public const string Yellow = "Yellow";
        public const string Blue = "Blue";
        public const string VolumeUp = "Volume Up";
        public const string VolumeDown = "VolumeD own";
        public const string ChannelUp = "Channel Up";
        public const string ChannelDown = "Channel Down";
        public const string Play = "Play";
        public const string Pause = "Pause";
        public const string Stop = "Stop";
        public const string Rewind = "Rewind";
        public const string FastForward = "Fast Forward";
        public const string PowerOff = "Power Off";
        public const string PowerOn = "Power On";
        public const string ToggleOnOff = "Toggle On/Off";

        public static IEnumerable<string> All()
        {
            var keys = new[]
            {
                Mute,
                Unmute,
                VolumeUp,
                VolumeDown,
                ChannelDown,
                ChannelUp,
                TurnOn3d,
                TurnOff3d,
                Home,
                Back,
                Up,
                Down,
                Left,
                Right,
                Red,
                Blue,
                Yellow,
                Green,
                FastForward,
                Pause,
                Play,
                Rewind,
                Stop,
                PowerOff,
                PowerOn,
                ToggleOnOff,
                Menu,
                Enter,
                Dash,
                Info,
                One,
                Two,
                Three,
                Four,
                Five,
                Six,
                Seven,
                Eight,
                Nine,
                Zero,
                Asterisk,
                Cc,
                Exit
            };

            return keys;
        }
    }
}