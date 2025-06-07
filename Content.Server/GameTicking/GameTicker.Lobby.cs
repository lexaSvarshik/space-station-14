using System.Linq;
using Content.Shared.GameTicking;
using Content.Server.Station.Components;
using Robust.Shared.Network;
using Robust.Shared.Player;
using System.Text;
using Content.Server.Administration.Managers;
using Content.Server.AlertLevel;
using Content.Server.Station.Systems;
using Content.Shared.Administration;

namespace Content.Server.GameTicking
{
    public sealed partial class GameTicker
    {
        [ViewVariables]
        private readonly Dictionary<NetUserId, PlayerGameStatus> _playerGameStatuses = new();

        [ViewVariables]
        private TimeSpan _roundStartTime;

        /// <summary>
        /// How long before RoundStartTime do we load maps.
        /// </summary>
        [ViewVariables]
        public TimeSpan RoundPreloadTime { get; } = TimeSpan.FromSeconds(15);

        [ViewVariables]
        private TimeSpan _pauseTime;

        [ViewVariables]
        public new bool Paused { get; set; }

        [ViewVariables]
        private bool _roundStartCountdownHasNotStartedYetDueToNoPlayers;

        [Dependency] private readonly IAdminManager _adminMgr = default!;
        [Dependency] private readonly StationSystem _stationSystem = default!; //ss220 add alert level in lobby start

        //ss220 add alert level in lobby start
        private readonly List<string> _defaultIgnoreAlertLevels = new()
        {
            "epsilon",
        };
        //ss220 add alert level in lobby end

        /// <summary>
        /// The game status of a players user Id. May contain disconnected players
        /// </summary>
        public IReadOnlyDictionary<NetUserId, PlayerGameStatus> PlayerGameStatuses => _playerGameStatuses;

        public void UpdateInfoText()
        {
            foreach (var session in _playerManager.NetworkedSessions)
            {
                RaiseNetworkEvent(GetInfoMsg(session), session.Channel);
            }
            // RaiseNetworkEvent(GetInfoMsg(), Filter.Empty().AddPlayers(_playerManager.NetworkedSessions));
        }

        private string GetInfoText(ICommonSession session)
        {
            var preset = CurrentPreset ?? Preset;
            if (preset == null)
            {
                return string.Empty;
            }

            var playerCount = $"{_playerManager.PlayerCount}";
            var readyCount = _playerGameStatuses.Values.Count(x => x == PlayerGameStatus.ReadyToPlay);

            var stationNames = new StringBuilder();
            var query =
                EntityQueryEnumerator<StationJobsComponent, StationSpawningComponent, MetaDataComponent>();

            var foundOne = false;

            while (query.MoveNext(out _, out _, out var meta))
            {
                foundOne = true;
                if (stationNames.Length > 0)
                        stationNames.Append('\n');

                stationNames.Append(meta.EntityName);
            }

            if (!foundOne)
            {
                stationNames.Append(_gameMapManager.GetSelectedMap()?.MapName ??
                                    Loc.GetString("game-ticker-no-map-selected"));
            }

            // SS220 Ограничение информации для пользователей о текущем режиме игры.
            // Для не администрации текущий режим всегда отображается как секрет.
            var isAdmin = _adminMgr.HasAdminFlag(session, AdminFlags.Admin);

            var gmTitle = isAdmin
                ? Loc.GetString(preset.ModeTitle)
                : Loc.GetString("secret-title");
            var desc = isAdmin
                ? Loc.GetString(preset.Description)
                : Loc.GetString("secret-description");

            //ss220 add alert level in lobby start
            var color = Color.Green;
            var level = "green";

            foreach (var station in _stationSystem.GetStations())
            {
                if (!HasComp<StationDataComponent>(station))
                    continue;

                if (!TryComp<AlertLevelComponent>(station, out var alertLevel))
                    continue;

                if (alertLevel.AlertLevels == null ||
                    !alertLevel.AlertLevels.Levels.TryGetValue(alertLevel.CurrentLevel, out var detail))
                    continue;

                if (_defaultIgnoreAlertLevels.Contains(alertLevel.CurrentLevel))
                    continue;

                color = detail.Color;
                level = alertLevel.CurrentLevel;
            }

            return Loc.GetString(
                RunLevel == GameRunLevel.PreRoundLobby
                    ? "game-ticker-get-info-preround-text"
                    : "game-ticker-get-info-text",
                ("roundId", RoundId),
                ("playerCount", playerCount),
                ("color", color.ToHex()),
                ("level", Loc.GetString($"alert-level-{level.ToLower()}")),
                ("readyCount", readyCount),
                ("mapName", stationNames.ToString()),
                ("gmTitle", gmTitle),
                ("desc", desc));
            //ss220 add alert level in lobby end
        }

        private TickerConnectionStatusEvent GetConnectionStatusMsg()
        {
            return new TickerConnectionStatusEvent(RoundStartTimeSpan);
        }

        private TickerLobbyStatusEvent GetStatusMsg(ICommonSession session)
        {
            _playerGameStatuses.TryGetValue(session.UserId, out var status);
            return new TickerLobbyStatusEvent(RunLevel != GameRunLevel.PreRoundLobby, LobbyBackground, status == PlayerGameStatus.ReadyToPlay, _roundStartTime, RoundPreloadTime, RoundStartTimeSpan, Paused);
        }

        private void SendStatusToAll()
        {
            foreach (var player in _playerManager.Sessions)
            {
                RaiseNetworkEvent(GetStatusMsg(player), player.Channel);
            }
        }

        private TickerLobbyInfoEvent GetInfoMsg(ICommonSession session)
        {
            return new (GetInfoText(session));
        }

        private void UpdateLateJoinStatus()
        {
            RaiseNetworkEvent(new TickerLateJoinStatusEvent(DisallowLateJoin));
        }

        public bool PauseStart(bool pause = true)
        {
            if (Paused == pause)
            {
                return false;
            }

            Paused = pause;

            if (pause)
            {
                _pauseTime = _gameTiming.CurTime;
            }
            else if (_pauseTime != default)
            {
                _roundStartTime += _gameTiming.CurTime - _pauseTime;
            }

            RaiseNetworkEvent(new TickerLobbyCountdownEvent(_roundStartTime, Paused));

            _chatManager.DispatchServerAnnouncement(Loc.GetString(Paused
                ? "game-ticker-pause-start"
                : "game-ticker-pause-start-resumed"));

            return true;
        }

        public bool TogglePause()
        {
            PauseStart(!Paused);
            return Paused;
        }

        public void ToggleReadyAll(bool ready)
        {
            var status = ready ? PlayerGameStatus.ReadyToPlay : PlayerGameStatus.NotReadyToPlay;
            foreach (var playerUserId in _playerGameStatuses.Keys)
            {
                _playerGameStatuses[playerUserId] = status;
                if (!_playerManager.TryGetSessionById(playerUserId, out var playerSession))
                    continue;
                RaiseNetworkEvent(GetStatusMsg(playerSession), playerSession.Channel);
            }
        }

        public void ToggleReady(ICommonSession player, bool ready)
        {
            if (!_playerGameStatuses.ContainsKey(player.UserId))
                return;

            if (!_userDb.IsLoadComplete(player))
                return;

            if (RunLevel != GameRunLevel.PreRoundLobby)
            {
                return;
            }

            var status = ready ? PlayerGameStatus.ReadyToPlay : PlayerGameStatus.NotReadyToPlay;
            _playerGameStatuses[player.UserId] = ready ? PlayerGameStatus.ReadyToPlay : PlayerGameStatus.NotReadyToPlay;
            RaiseNetworkEvent(GetStatusMsg(player), player.Channel);
            // update server info to reflect new ready count
            UpdateInfoText();
        }

        public bool UserHasJoinedGame(ICommonSession session)
            => UserHasJoinedGame(session.UserId);

        public bool UserHasJoinedGame(NetUserId userId)
            => PlayerGameStatuses.TryGetValue(userId, out var status) && status == PlayerGameStatus.JoinedGame;
    }
}
