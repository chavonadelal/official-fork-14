using Content.Server.AlertLevel;
using Content.Server.Audio;
using Content.Server.Chat.Systems;
using Content.Server.Station.Systems;
using Robust.Server.GameObjects;
using Robust.Server.Maps;
using Robust.Server.Player;
using Robust.Shared.Audio;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using System.Numerics;

namespace Content.Server.SendShuttle;
public sealed class SendShuttleSystem : EntitySystem
{
    [Dependency] private readonly IEntitySystemManager _entitySystem = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly MapSystem _mapSystem = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;
    [Dependency] private readonly ServerGlobalSoundSystem _serverGlobalSound = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly AlertLevelSystem _alertLevel = default!;
    private readonly ISawmill _sendShuttleSawmill = default!;

    private void SpawnMapAndGrid(SendShuttlePrototype proto)
    {
        var map = _mapSystem.CreateMap(out var mapId);
        _metaData.SetEntityName(map, Loc.GetString("sent-shuttle-map-name"));

        var girdOptions = new MapLoadOptions
        {
            Offset = new Vector2(0, 0),
            Rotation = Angle.FromDegrees(0)
        };

        if (!string.IsNullOrWhiteSpace(proto.GridPath.ToString()))
        {
            _mapLoader.Load(mapId, proto.GridPath.ToString(), girdOptions);
        }
        else
        {
            _sendShuttleSawmill.Error($"DataField GridPath is null or white space in SendShuttlePrototype with id: {proto.ID} ");
            return;
        }
    }

    public void SpawnShuttle(string sendShuttle, bool playAnnonce)
    {
        var protos = IoCManager.Resolve<IPrototypeManager>();
        var shuttleProto = protos.Index<SendShuttlePrototype>(sendShuttle);
        SpawnMapAndGrid(shuttleProto);

        if (playAnnonce && shuttleProto.SendAnnouncement)
        {
            if (!string.IsNullOrWhiteSpace(shuttleProto.SetAlertLevel))
                SetAlertLevel(shuttleProto);

            if (shuttleProto.IsPlayCustomAudio)
                PlayAudioAnnonce(shuttleProto);

            WriteAnnonce(shuttleProto);
        }
    }

    private void PlayAudioAnnonce(SendShuttlePrototype proto)
    {
        var filter = Filter.Empty().AddAllPlayers(_playerManager);
        var audioOption = AudioParams.Default;
        var filename = proto.SoundAnnounce.ToString();

        if (filename != null && proto.SoundAnnounce != null)
        {
            audioOption = audioOption.WithVolume(proto.SoundAnnounce.Params.Volume);
            _serverGlobalSound.PlayAdminGlobal(filter, filename, audioOption, true);
        }
    }

    private void WriteAnnonce(SendShuttlePrototype proto)
    {
        if (!string.IsNullOrWhiteSpace(proto.AnnouncementText))
        {
            _chat.DispatchGlobalAnnouncement(
            Loc.GetString($"{proto.AnnouncementText}"),
            string.IsNullOrWhiteSpace(proto.AnnouncementSender)
            ? null
            : Loc.GetString($"{proto.AnnouncementSender}"),
            playSound: proto.IsPlayAuidoFromAnnouncement,
            colorOverride: proto.AnnouncementColor);
        }
        else
        {
            _sendShuttleSawmill.Error($"DataField AnnouncementText is null or white space in SendShuttlePrototype with id: {proto.ID} ");
        }
    }

    private void SetAlertLevel(SendShuttlePrototype proto)
    {
        var stationUids = _entitySystem.GetEntitySystem<StationSystem>().GetStations();
        if (stationUids == null)
            return;

        foreach (var stationUid in stationUids)
        {
            _alertLevel.SetLevel(stationUid, proto.SetAlertLevel, proto.PlayAlertLevelSound, true,
                                                                 true, true);
        }

        return;
    }
}
