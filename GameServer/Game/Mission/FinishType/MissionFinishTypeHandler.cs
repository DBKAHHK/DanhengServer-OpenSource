using EggLink.DanhengServer.Data.Config;
using EggLink.DanhengServer.GameServer.Game.Player;

namespace EggLink.DanhengServer.GameServer.Game.Mission.FinishType;

public abstract class MissionFinishTypeHandler
{
    public abstract ValueTask HandleFinishType(PlayerInstance player, SubMissionInfo info, object? arg);
}