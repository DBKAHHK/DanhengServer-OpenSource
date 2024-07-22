using EggLink.DanhengServer.Data.Config;
using EggLink.DanhengServer.Enums;
using EggLink.DanhengServer.Game.Player;

namespace EggLink.DanhengServer.Game.Mission.FinishType.Handler;

[MissionFinishType(MissionFinishTypeEnum.NotInFloor)]
public class MissionHandlerNotInFloor : MissionFinishTypeHandler
{
    public override async ValueTask HandleFinishType(PlayerInstance player, SubMissionInfo info, object? arg)
    {
        if (player.Data.FloorId != info.ParamInt1) await player.MissionManager!.FinishSubMission(info.ID);
    }
}