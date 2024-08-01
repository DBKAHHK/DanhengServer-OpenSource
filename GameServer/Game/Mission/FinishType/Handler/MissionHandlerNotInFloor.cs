using EggLink.DanhengServer.Data.Config;
using EggLink.DanhengServer.Enums.Mission;
using EggLink.DanhengServer.GameServer.Game.Player;

namespace EggLink.DanhengServer.GameServer.Game.Mission.FinishType.Handler;

[MissionFinishType(MissionFinishTypeEnum.NotInFloor)]
public class MissionHandlerNotInFloor : MissionFinishTypeHandler
{
    public override async ValueTask HandleFinishType(PlayerInstance player, SubMissionInfo info, object? arg)
    {
        if (player.Data.FloorId != info.ParamInt1) await player.MissionManager!.FinishSubMission(info.ID);
    }
}