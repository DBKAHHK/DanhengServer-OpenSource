using EggLink.DanhengServer.Data.Config;
using EggLink.DanhengServer.Enums;
using EggLink.DanhengServer.Game.Player;

namespace EggLink.DanhengServer.Game.Mission.FinishType.Handler;

[MissionFinishType(MissionFinishTypeEnum.EnterMapByEntrance)]
public class MissionHandlerEnterMapByEntrance : MissionFinishTypeHandler
{
    public override async ValueTask HandleFinishType(PlayerInstance player, SubMissionInfo info, object? arg)
    {
        if (arg is int v)
            if (v == info.ParamInt1)
                await player.MissionManager!.FinishSubMission(info.ID);
    }
}