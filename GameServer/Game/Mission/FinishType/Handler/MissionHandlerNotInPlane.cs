using EggLink.DanhengServer.Data.Config;
using EggLink.DanhengServer.Enums.Mission;
using EggLink.DanhengServer.GameServer.Game.Player;

namespace EggLink.DanhengServer.GameServer.Game.Mission.FinishType.Handler;

[MissionFinishType(MissionFinishTypeEnum.NotInPlane)]
public class MissionHandlerNotInPlane : MissionFinishTypeHandler
{
    public override async ValueTask HandleFinishType(PlayerInstance player, SubMissionInfo info, object? arg)
    {
        if (player.Data.PlaneId == info.ParamInt1) return; // not a same scene
        await player.MissionManager!.FinishSubMission(info.ID);
    }
}