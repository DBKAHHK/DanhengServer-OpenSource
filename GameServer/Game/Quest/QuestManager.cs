using EggLink.DanhengServer.GameServer.Game.Player;

namespace EggLink.DanhengServer.GameServer.Game.Quest;

public class QuestManager(PlayerInstance player) : BasePlayerManager(player)
{
    public UnlockHandler UnlockHandler { get; } = new(player);
}