using Raylib_cs;

namespace OGS.Client.Desktop;

public enum ClientCommandType : byte
{
    Unknown = 0,
    MouseMove = 1,
    MouseButton = 2,
    MouseScroll = 3,
    KeyboardKey = 4,
    GamepadAxis = 5,
    GamepadButton = 6,
}

public enum MouseButtonCommand : byte
{
    None = 0,
    Left = 1,
    Right = 2,
    Middle = 3,
    X1 = 4,
    X2 = 5,
}

public enum ScrollDirection : byte { None = 0, Up = 1, Down = 2, Left = 3, Right = 4 }

public static class InputProtocol
{
    public static MouseButtonCommand MouseButtonFromRaylib(MouseButton button) => button switch
    {
        MouseButton.MouseButtonLeft => MouseButtonCommand.Left,
        MouseButton.MouseButtonRight => MouseButtonCommand.Right,
        MouseButton.MouseButtonMiddle => MouseButtonCommand.Middle,
        MouseButton.MouseButtonSide => MouseButtonCommand.X1,
        MouseButton.MouseButtonExtra => MouseButtonCommand.X2,
        _ => MouseButtonCommand.None,
    };
}

public interface IRemoteCommandTransport
{
    void SendMouseMove(short x, short y);
    void SendMouseButton(MouseButtonCommand button, bool pressed);
    void SendMouseScroll(ScrollDirection direction);
    void SendKeyboardKey(byte keyCode, bool pressed);
}

public sealed class NullRemoteCommandTransport : IRemoteCommandTransport
{
    public void SendKeyboardKey(byte keyCode, bool pressed) { }
    public void SendMouseButton(MouseButtonCommand button, bool pressed) { }
    public void SendMouseMove(short x, short y) { }
    public void SendMouseScroll(ScrollDirection direction) { }
}
