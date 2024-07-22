using EggLink.DanhengServer.Game.Player;

namespace EggLink.DanhengServer.Game;

public class BasePlayerManager(PlayerInstance player)
{
    public PlayerInstance Player { get; private set; } = player;
}