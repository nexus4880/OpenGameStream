using System.Text.Json;
using Raylib_cs;

namespace OGS.Client.Desktop;

public sealed class SettingsStore
{
    private readonly string _path = Path.Combine(AppContext.BaseDirectory, "desktop-client.settings.json");

    public ClientSettings Load()
    {
        if (!File.Exists(_path))
            return new ClientSettings();

        string json = File.ReadAllText(_path);
        return JsonSerializer.Deserialize<ClientSettings>(json) ?? new ClientSettings();
    }

    public void Save(ClientSettings settings)
    {
        string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_path, json);
    }
}

public sealed class ClientSettings
{
    public KeyboardKey ExitKey { get; set; } = KeyboardKey.F1;
}
