using System.Numerics;
using Raylib_cs;

namespace OGS.Client.Desktop;

public sealed class DesktopClientApp
{
    private readonly SettingsStore _settings = new();
    private readonly NullRemoteCommandTransport _transport = new();

    private AppScreen _screen = AppScreen.Setup;
    private InviteData? _invite;
    private string? _error;

    public void Run()
    {
        ClientSettings settings = _settings.Load();

        Raylib.SetConfigFlags(ConfigFlags.ResizableWindow | ConfigFlags.Msaa4xHint);
        Raylib.InitWindow(1280, 720, "OGS Desktop Client");
        Raylib.SetTargetFPS(240);

        bool captureInput = false;

        while (!Raylib.WindowShouldClose())
        {
            switch (_screen)
            {
                case AppScreen.Setup:
                    DrawSetupUi(settings);
                    break;
                case AppScreen.InviteInfo:
                    DrawInviteInfo(settings);
                    break;
                case AppScreen.Session:
                    if (Raylib.IsKeyPressed(settings.ExitKey))
                        captureInput = !captureInput;

                    if (captureInput)
                        PollAndForwardInput(settings.ExitKey);

                    DrawSessionUi(captureInput, settings.ExitKey);
                    break;
            }
        }

        Raylib.CloseWindow();
    }

    private void PollAndForwardInput(KeyboardKey exitKey)
    {
        Vector2 delta = Raylib.GetMouseDelta();
        if (delta.X != 0 || delta.Y != 0)
            _transport.SendMouseMove((short)delta.X, (short)delta.Y);

        if (Raylib.IsMouseButtonPressed(MouseButton.MouseButtonLeft)) _transport.SendMouseButton(MouseButtonCommand.Left, true);
        if (Raylib.IsMouseButtonReleased(MouseButton.MouseButtonLeft)) _transport.SendMouseButton(MouseButtonCommand.Left, false);
        if (Raylib.IsMouseButtonPressed(MouseButton.MouseButtonRight)) _transport.SendMouseButton(MouseButtonCommand.Right, true);
        if (Raylib.IsMouseButtonReleased(MouseButton.MouseButtonRight)) _transport.SendMouseButton(MouseButtonCommand.Right, false);
        if (Raylib.IsMouseButtonPressed(MouseButton.MouseButtonMiddle)) _transport.SendMouseButton(MouseButtonCommand.Middle, true);
        if (Raylib.IsMouseButtonReleased(MouseButton.MouseButtonMiddle)) _transport.SendMouseButton(MouseButtonCommand.Middle, false);

        int wheel = (int)Raylib.GetMouseWheelMove();
        if (wheel > 0) _transport.SendMouseScroll(ScrollDirection.Up);
        if (wheel < 0) _transport.SendMouseScroll(ScrollDirection.Down);

        for (int key = (int)KeyboardKey.Null; key < (int)KeyboardKey.KbMenu; key++)
        {
            if (key == (int)exitKey)
                continue;

            if (Raylib.IsKeyPressed((KeyboardKey)key)) _transport.SendKeyboardKey((byte)key, true);
            if (Raylib.IsKeyReleased((KeyboardKey)key)) _transport.SendKeyboardKey((byte)key, false);
        }
    }

    private void DrawSetupUi(ClientSettings settings)
    {
        if (Raylib.IsKeyPressed(KeyboardKey.F1)) settings.ExitKey = KeyboardKey.F1;
        if (Raylib.IsKeyPressed(KeyboardKey.F2)) settings.ExitKey = KeyboardKey.F2;
        if (Raylib.IsKeyPressed(KeyboardKey.F3)) settings.ExitKey = KeyboardKey.F3;
        if (Raylib.IsKeyPressed(KeyboardKey.F4)) settings.ExitKey = KeyboardKey.F4;

        if (Raylib.IsKeyPressed(KeyboardKey.V) && (Raylib.IsKeyDown(KeyboardKey.LeftControl) || Raylib.IsKeyDown(KeyboardKey.RightControl)))
        {
            TryReadInviteFromClipboard();
            if (_invite is not null)
            {
                _settings.Save(settings);
                _screen = AppScreen.InviteInfo;
            }
        }

        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.Black);
        Raylib.DrawText("OpenGameStream Desktop Client", 40, 40, 38, Color.White);
        Raylib.DrawText("1) Choose a dedicated exit key. 2) Press CTRL+V to paste invite code.", 40, 95, 20, Color.LightGray);

        Raylib.DrawRectangleLines(40, 150, 420, 50, Color.Gray);
        Raylib.DrawText($"Exit key: {settings.ExitKey}", 55, 165, 24, Color.Gold);
        Raylib.DrawText("Press F1/F2/F3/F4 to choose", 40, 220, 18, Color.LightGray);

        Raylib.DrawText("Paste invite: CTRL+V", 40, 260, 24, Color.Green);

        if (!string.IsNullOrWhiteSpace(_error))
            Raylib.DrawText(_error, 40, 310, 20, Color.Red);

        Raylib.EndDrawing();
    }

    private void DrawInviteInfo(ClientSettings settings)
    {
        if (_invite is null)
        {
            _screen = AppScreen.Setup;
            return;
        }

        if (Raylib.IsKeyPressed(KeyboardKey.Enter))
        {
            // TODO: construct and start real RTC client from invite (manual/MQTT).
            _screen = AppScreen.Session;
        }

        if (Raylib.IsKeyPressed(KeyboardKey.Backspace))
        {
            _screen = AppScreen.Setup;
        }

        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.Black);
        Raylib.DrawText("Host information", 40, 40, 38, Color.White);

        if (_invite is ManualRtcInviteData)
            Raylib.DrawText("Type: WebRTC via manual signalling", 40, 105, 24, Color.LightGray);

        if (_invite is MqttRtcInviteData mqtt)
        {
            Raylib.DrawText("Type: WebRTC via MQTT signalling", 40, 105, 24, Color.LightGray);
            Raylib.DrawText($"MQTT URL: {mqtt.WebsocketUrl}", 40, 145, 20, Color.LightGray);
        }

        Raylib.DrawText($"Exit key: {settings.ExitKey}", 40, 195, 20, Color.Gold);
        Raylib.DrawText("ENTER = Connect    BACKSPACE = Back", 40, 240, 24, Color.Green);
        Raylib.EndDrawing();
    }

    private static void DrawSessionUi(bool captureInput, KeyboardKey exitKey)
    {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(captureInput ? Color.DarkBlue : Color.DarkGray);
        Raylib.DrawText("Session active", 40, 40, 34, Color.White);
        Raylib.DrawText($"Input routing: {(captureInput ? "HOST" : "LOCAL")}", 40, 90, 24, Color.Yellow);
        Raylib.DrawText($"Press {exitKey} to toggle/cancel capture.", 40, 130, 20, Color.LightGray);
        Raylib.EndDrawing();
    }

    private void TryReadInviteFromClipboard()
    {
        try
        {
            string raw = Raylib.GetClipboardText();
            _invite = InviteData.Parse(raw.Trim());
            _error = null;
        }
        catch (Exception ex)
        {
            _invite = null;
            _error = $"Invalid invite code: {ex.Message}";
        }
    }
}

public enum AppScreen
{
    Setup,
    InviteInfo,
    Session
}
