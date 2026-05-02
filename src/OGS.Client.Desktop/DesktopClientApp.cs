using System.Numerics;
using Raylib_cs;

namespace OGS.Client.Desktop;

public sealed class DesktopClientApp
{
    private readonly SettingsStore _settings = new();
    private readonly NullRemoteCommandTransport _transport = new();

    public void Run()
    {
        ClientSettings settings = _settings.Load();

        Raylib.SetConfigFlags(ConfigFlags.ResizableWindow | ConfigFlags.Msaa4xHint);
        Raylib.InitWindow(1280, 720, "OGS Desktop Client");
        Raylib.SetTargetFPS(240);

        bool connected = false;
        bool captureInput = false;

        while (!Raylib.WindowShouldClose())
        {
            if (!connected)
            {
                DrawSetupUi(settings, ref connected);
                if (connected)
                {
                    _settings.Save(settings);
                    captureInput = true;
                }

                continue;
            }

            if (Raylib.IsKeyPressed(settings.ExitKey))
            {
                captureInput = !captureInput;
            }

            if (captureInput)
            {
                PollAndForwardInput();
            }

            DrawSessionUi(captureInput, settings.ExitKey);
        }

        Raylib.CloseWindow();
    }

    private void PollAndForwardInput()
    {
        Vector2 delta = Raylib.GetMouseDelta();
        if (delta.X != 0 || delta.Y != 0)
            _transport.SendMouseMove((short)delta.X, (short)delta.Y);

        foreach (MouseButton mb in Enum.GetValues<MouseButton>())
        {
            if (mb == MouseButton.MouseButtonLeft || mb == MouseButton.MouseButtonRight || mb == MouseButton.MouseButtonMiddle)
            {
                if (Raylib.IsMouseButtonPressed(mb)) _transport.SendMouseButton(InputProtocol.MouseButtonFromRaylib(mb), true);
                if (Raylib.IsMouseButtonReleased(mb)) _transport.SendMouseButton(InputProtocol.MouseButtonFromRaylib(mb), false);
            }
        }

        int wheel = (int)Raylib.GetMouseWheelMove();
        if (wheel > 0) _transport.SendMouseScroll(ScrollDirection.Up);
        if (wheel < 0) _transport.SendMouseScroll(ScrollDirection.Down);

        for (int key = (int)KeyboardKey.Null; key < (int)KeyboardKey.KbMenu; key++)
        {
            if (Raylib.IsKeyPressed((KeyboardKey)key)) _transport.SendKeyboardKey((byte)key, true);
            if (Raylib.IsKeyReleased((KeyboardKey)key)) _transport.SendKeyboardKey((byte)key, false);
        }
    }

    private static void DrawSetupUi(ClientSettings settings, ref bool connected)
    {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.Black);
        Raylib.DrawText("OGS Desktop Client", 40, 40, 38, Color.White);
        Raylib.DrawText("Set the dedicated exit keybind before connecting.", 40, 100, 20, Color.LightGray);

        Raylib.DrawRectangleLines(40, 150, 360, 50, Color.Gray);
        Raylib.DrawText($"Exit key: {settings.ExitKey}", 55, 165, 24, Color.Gold);
        Raylib.DrawText("Press F1/F2/F3/F4 to choose", 40, 215, 18, Color.LightGray);

        if (Raylib.IsKeyPressed(KeyboardKey.F1)) settings.ExitKey = KeyboardKey.F1;
        if (Raylib.IsKeyPressed(KeyboardKey.F2)) settings.ExitKey = KeyboardKey.F2;
        if (Raylib.IsKeyPressed(KeyboardKey.F3)) settings.ExitKey = KeyboardKey.F3;
        if (Raylib.IsKeyPressed(KeyboardKey.F4)) settings.ExitKey = KeyboardKey.F4;

        Raylib.DrawText("Press ENTER to connect", 40, 270, 26, Color.Green);
        if (Raylib.IsKeyPressed(KeyboardKey.Enter)) connected = true;

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
}
