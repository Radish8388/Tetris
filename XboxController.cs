using System.Runtime.InteropServices;

namespace Tetris
{
    public class XboxController
    {
        private readonly uint _index;
        private XInputState _previousState;
        private bool _isConnected;

        public XboxController(uint index)
        {
            _index = index;
        }

        public bool IsConnected => _isConnected;

        // Held-down state for the current poll
        public XInputButtons ButtonsHeld { get; private set; }

        // True only on the poll where a button transitions from up to down
        public XInputButtons ButtonsPressedThisPoll { get; private set; }

        public double LeftStickX { get; private set; }
        public double LeftStickY { get; private set; }
        public double RightStickX { get; private set; }
        public double RightStickY { get; private set; }
        public double LeftTrigger { get; private set; }
        public double RightTrigger { get; private set; }

        public void Poll()
        {
            int result = XInput.XInputGetState(_index, out XInputState state);
            _isConnected = (result == 0);

            if (!_isConnected)
            {
                ButtonsHeld = 0;
                ButtonsPressedThisPoll = 0;
                return;
            }

            var currentButtons = (XInputButtons)state.Gamepad.wButtons;
            var previousButtons = (XInputButtons)_previousState.Gamepad.wButtons;

            ButtonsPressedThisPoll = currentButtons & ~previousButtons; // newly pressed since last poll
            ButtonsHeld = currentButtons;

            LeftStickX = state.Gamepad.sThumbLX / 32768.0;
            LeftStickY = state.Gamepad.sThumbLY / 32768.0;
            RightStickX = state.Gamepad.sThumbRX / 32768.0;
            RightStickY = state.Gamepad.sThumbRY / 32768.0;
            LeftTrigger = state.Gamepad.bLeftTrigger / 255.0;
            RightTrigger = state.Gamepad.bRightTrigger / 255.0;

            _previousState = state;
        }

        public bool IsButtonHeld(XInputButtons button) => ButtonsHeld.HasFlag(button);
        public bool WasButtonJustPressed(XInputButtons button) => ButtonsPressedThisPoll.HasFlag(button);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XInputGamepad
    {
        public ushort wButtons;
        public byte bLeftTrigger;
        public byte bRightTrigger;
        public short sThumbLX;
        public short sThumbLY;
        public short sThumbRX;
        public short sThumbRY;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XInputState
    {
        public uint dwPacketNumber;
        public XInputGamepad Gamepad;
    }

    public static class XInput
    {
        [DllImport("xinput1_4.dll")]
        public static extern int XInputGetState(uint dwUserIndex, out XInputState pState);
    }

    public enum XInputButtons : ushort
    {
        DPadUp = 0x0001,
        DPadDown = 0x0002,
        DPadLeft = 0x0004,
        DPadRight = 0x0008,
        Start = 0x0010,
        Back = 0x0020,
        LeftThumb = 0x0040,
        RightThumb = 0x0080,
        LeftShoulder = 0x0100,
        RightShoulder = 0x0200,
        A = 0x1000,
        B = 0x2000,
        X = 0x4000,
        Y = 0x8000
    }
}
