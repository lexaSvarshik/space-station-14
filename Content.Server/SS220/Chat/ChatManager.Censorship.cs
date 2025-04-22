using Content.Shared.Database;
using Content.Shared.Mind;
using Robust.Shared.Player;
using System.Text.RegularExpressions;

namespace Content.Server.Chat.Managers;

internal sealed partial class ChatManager
{
    [GeneratedRegex(@"[\u0fd5-\u0fd8\u2500-\u25ff\u2800-\u28ff]+")]
    private static partial Regex ProhibitedCharactersRegex();

    public string DeleteProhibitedCharacters(string message, EntityUid player)
    {
        var mindSystem = _entityManager.System<SharedMindSystem>();
        mindSystem.TryGetMind(player, out _, out var mind);

        return DeleteProhibitedCharacters(message, mind?.Session);
    }

    public string DeleteProhibitedCharacters(string message, ICommonSession? player = null)
    {
        string censoredMessage = ProhibitedCharactersRegex().Replace(message, string.Empty);
        if (message.Length != censoredMessage.Length && player != null)
        {
            _adminLogger.Add(LogType.Chat, LogImpact.Low, $"{player.Name} tried to send a message with forbidden characters:\n{message}");
            SendAdminAlert(Loc.GetString("chat-manager-founded-prohibited-characters", ("player", player.Name), ("message", message)));
        }

        return censoredMessage;
    }
}
