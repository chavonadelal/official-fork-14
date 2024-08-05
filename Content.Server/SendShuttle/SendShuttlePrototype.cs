using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server.SendShuttle;

[Prototype]
public sealed partial class SendShuttlePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; } = default!;

    [DataField]
    public bool SendAnnouncement = true;

    [DataField]
    public string AnnouncementText = string.Empty;

    [DataField]
    public string AnnouncementSender = string.Empty;

    [DataField]
    public Color AnnouncementColor = Color.Gold;

    [DataField]
    public bool IsPlayAuidoFromAnnouncement = false;

    [DataField]
    public bool IsPlayCustomAudio = true;

    [DataField]
    public SoundSpecifier SoundAnnounce = new SoundPathSpecifier("Audio/Announcements/attention.ogg");

    [DataField]
    public string SetAlertLevel = string.Empty;

    [DataField]
    public bool PlayAlertLevelSound = true;

    [DataField]
    public ResPath GridPath;

    [DataField]
    public string HintText = string.Empty;
}
