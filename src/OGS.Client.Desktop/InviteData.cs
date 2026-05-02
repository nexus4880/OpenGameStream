using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OGS.Client.Desktop;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(ManualRtcInviteData), typeDiscriminator: "ManualRtc")]
[JsonDerivedType(typeof(MqttRtcInviteData), typeDiscriminator: "MQTT")]
public abstract class InviteData
{
    public static InviteData Parse(string inviteCode)
    {
        byte[] decoded = Convert.FromBase64String(inviteCode);
        string json = Encoding.UTF8.GetString(decoded);

        InviteData? invite = JsonSerializer.Deserialize<InviteData>(json);
        if (invite is null)
            throw new InvalidOperationException("Unable to parse invite payload.");

        return invite;
    }
}

public sealed class ManualRtcInviteData : InviteData
{
    public required string Sdp { get; set; }
}

public sealed class MqttRtcInviteData : InviteData
{
    public required string WebsocketUrl { get; set; }
    public required string HostTopic { get; set; }
    public required string ClientTopic { get; set; }
    public required string AesKey { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
}
