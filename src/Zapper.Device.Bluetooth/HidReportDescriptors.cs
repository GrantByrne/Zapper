namespace Zapper.Device.Bluetooth;

public static class HidReportDescriptors
{
    public static byte[] GetRemoteControlDescriptor()
    {
        return
        [
            0x05, 0x0C,        // Usage Page (Consumer)
            0x09, 0x01,        // Usage (Consumer Control)
            0xA1, 0x01,        // Collection (Application)
            0x85, 0x01,        //   Report ID (1)
            0x19, 0x00,        //   Usage Minimum (0)
            0x2A, 0x3C, 0x02,  //   Usage Maximum (0x023C)
            0x15, 0x00,        //   Logical Minimum (0)
            0x26, 0x3C, 0x02,  //   Logical Maximum (0x023C)
            0x95, 0x01,        //   Report Count (1)
            0x75, 0x10,        //   Report Size (16)
            0x81, 0x00,        //   Input (Data,Array,Abs)
            0xC0,              // End Collection
            
            0x05, 0x01,        // Usage Page (Generic Desktop)
            0x09, 0x06,        // Usage (Keyboard)
            0xA1, 0x01,        // Collection (Application)
            0x85, 0x02,        //   Report ID (2)
            0x05, 0x07,        //   Usage Page (Keyboard)
            0x19, 0xE0,        //   Usage Minimum (Left Control)
            0x29, 0xE7,        //   Usage Maximum (Right GUI)
            0x15, 0x00,        //   Logical Minimum (0)
            0x25, 0x01,        //   Logical Maximum (1)
            0x75, 0x01,        //   Report Size (1)
            0x95, 0x08,        //   Report Count (8)
            0x81, 0x02,        //   Input (Data,Var,Abs)
            0x95, 0x01,        //   Report Count (1)
            0x75, 0x08,        //   Report Size (8)
            0x81, 0x01,        //   Input (Const,Array,Abs)
            0x95, 0x05,        //   Report Count (5)
            0x75, 0x01,        //   Report Size (1)
            0x05, 0x08,        //   Usage Page (LEDs)
            0x19, 0x01,        //   Usage Minimum (Num Lock)
            0x29, 0x05,        //   Usage Maximum (Kana)
            0x91, 0x02,        //   Output (Data,Var,Abs)
            0x95, 0x01,        //   Report Count (1)
            0x75, 0x03,        //   Report Size (3)
            0x91, 0x01,        //   Output (Const,Array,Abs)
            0x95, 0x06,        //   Report Count (6)
            0x75, 0x08,        //   Report Size (8)
            0x15, 0x00,        //   Logical Minimum (0)
            0x25, 0x65,        //   Logical Maximum (101)
            0x05, 0x07,        //   Usage Page (Keyboard)
            0x19, 0x00,        //   Usage Minimum (0)
            0x29, 0x65,        //   Usage Maximum (101)
            0x81, 0x00,        //   Input (Data,Array,Abs)
            0xC0               // End Collection
        ];
    }

    public static byte[] GetGamepadDescriptor()
    {
        return
        [
            0x05, 0x01,        // Usage Page (Generic Desktop)
            0x09, 0x05,        // Usage (Game Pad)
            0xA1, 0x01,        // Collection (Application)
            0x85, 0x03,        //   Report ID (3)
            
            // Buttons
            0x05, 0x09,        //   Usage Page (Button)
            0x19, 0x01,        //   Usage Minimum (Button 1)
            0x29, 0x10,        //   Usage Maximum (Button 16)
            0x15, 0x00,        //   Logical Minimum (0)
            0x25, 0x01,        //   Logical Maximum (1)
            0x95, 0x10,        //   Report Count (16)
            0x75, 0x01,        //   Report Size (1)
            0x81, 0x02,        //   Input (Data,Var,Abs)
            
            // D-Pad
            0x05, 0x01,        //   Usage Page (Generic Desktop)
            0x09, 0x39,        //   Usage (Hat switch)
            0x15, 0x00,        //   Logical Minimum (0)
            0x25, 0x07,        //   Logical Maximum (7)
            0x35, 0x00,        //   Physical Minimum (0)
            0x46, 0x3B, 0x01,  //   Physical Maximum (315)
            0x65, 0x14,        //   Unit (Degrees)
            0x75, 0x04,        //   Report Size (4)
            0x95, 0x01,        //   Report Count (1)
            0x81, 0x42,        //   Input (Data,Var,Abs,Null State)
            0x65, 0x00,        //   Unit (None)
            0x95, 0x01,        //   Report Count (1)
            0x75, 0x04,        //   Report Size (4)
            0x81, 0x01,        //   Input (Const,Array,Abs)
            
            // Analog sticks
            0x09, 0x30,        //   Usage (X)
            0x09, 0x31,        //   Usage (Y)
            0x09, 0x32,        //   Usage (Z)
            0x09, 0x35,        //   Usage (Rz)
            0x15, 0x00,        //   Logical Minimum (0)
            0x26, 0xFF, 0x00,  //   Logical Maximum (255)
            0x75, 0x08,        //   Report Size (8)
            0x95, 0x04,        //   Report Count (4)
            0x81, 0x02,        //   Input (Data,Var,Abs)
            
            // Triggers
            0x09, 0x33,        //   Usage (Rx)
            0x09, 0x34,        //   Usage (Ry)
            0x15, 0x00,        //   Logical Minimum (0)
            0x26, 0xFF, 0x00,  //   Logical Maximum (255)
            0x75, 0x08,        //   Report Size (8)
            0x95, 0x02,        //   Report Count (2)
            0x81, 0x02,        //   Input (Data,Var,Abs)
            
            0xC0               // End Collection
        ];
    }

    public static byte[] CreateRemoteKeyReport(HidKeyCode keyCode, bool pressed)
    {
        var report = new byte[3];
        report[0] = 0x01; // Report ID for consumer control

        if (pressed)
        {
            var consumerCode = GetConsumerControlCode(keyCode);
            report[1] = (byte)(consumerCode & 0xFF);
            report[2] = (byte)((consumerCode >> 8) & 0xFF);
        }

        return report;
    }

    public static byte[] CreateKeyboardReport(HidKeyCode keyCode, bool pressed)
    {
        var report = new byte[9];
        report[0] = 0x02; // Report ID for keyboard

        if (pressed)
        {
            var scanCode = GetKeyboardScanCode(keyCode);
            if (scanCode > 0)
            {
                report[3] = scanCode; // First key slot
            }
        }

        return report;
    }

    private static ushort GetConsumerControlCode(HidKeyCode keyCode)
    {
        return keyCode switch
        {
            HidKeyCode.PlayPause => 0x00CD,
            HidKeyCode.Stop => 0x00B7,
            HidKeyCode.FastForward => 0x00B3,
            HidKeyCode.Rewind => 0x00B4,
            HidKeyCode.VolumeUp => 0x00E9,
            HidKeyCode.VolumeDown => 0x00EA,
            HidKeyCode.VolumeMute => 0x00E2,
            HidKeyCode.Menu => 0x0040,
            HidKeyCode.Home => 0x0223,
            HidKeyCode.Back => 0x0224,
            HidKeyCode.Search => 0x0221,
            _ => 0x0000
        };
    }

    private static byte GetKeyboardScanCode(HidKeyCode keyCode)
    {
        return keyCode switch
        {
            HidKeyCode.DPadUp => 0x52,    // Up Arrow
            HidKeyCode.DPadDown => 0x51,  // Down Arrow
            HidKeyCode.DPadLeft => 0x50,  // Left Arrow
            HidKeyCode.DPadRight => 0x4F, // Right Arrow
            HidKeyCode.DPadCenter => 0x28, // Enter
            HidKeyCode.Escape => 0x29,
            HidKeyCode.Tab => 0x2B,
            HidKeyCode.Space => 0x2C,
            HidKeyCode.Backspace => 0x2A,
            HidKeyCode.Key0 => 0x27,
            HidKeyCode.Key1 => 0x1E,
            HidKeyCode.Key2 => 0x1F,
            HidKeyCode.Key3 => 0x20,
            HidKeyCode.Key4 => 0x21,
            HidKeyCode.Key5 => 0x22,
            HidKeyCode.Key6 => 0x23,
            HidKeyCode.Key7 => 0x24,
            HidKeyCode.Key8 => 0x25,
            HidKeyCode.Key9 => 0x26,
            HidKeyCode.A => 0x04,
            HidKeyCode.B => 0x05,
            HidKeyCode.C => 0x06,
            HidKeyCode.D => 0x07,
            HidKeyCode.E => 0x08,
            HidKeyCode.F => 0x09,
            HidKeyCode.G => 0x0A,
            HidKeyCode.H => 0x0B,
            HidKeyCode.I => 0x0C,
            HidKeyCode.J => 0x0D,
            HidKeyCode.K => 0x0E,
            HidKeyCode.L => 0x0F,
            HidKeyCode.M => 0x10,
            HidKeyCode.N => 0x11,
            HidKeyCode.O => 0x12,
            HidKeyCode.P => 0x13,
            HidKeyCode.Q => 0x14,
            HidKeyCode.R => 0x15,
            HidKeyCode.S => 0x16,
            HidKeyCode.T => 0x17,
            HidKeyCode.U => 0x18,
            HidKeyCode.V => 0x19,
            HidKeyCode.W => 0x1A,
            HidKeyCode.X => 0x1B,
            HidKeyCode.Y => 0x1C,
            HidKeyCode.Z => 0x1D,
            _ => 0x00
        };
    }
}