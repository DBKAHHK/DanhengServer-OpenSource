using EggLink.DanhengServer.Data.Config;
using EggLink.DanhengServer.Enums;
using EggLink.DanhengServer.Game.Player;
using EggLink.DanhengServer.Game.Scene.Entity;

namespace EggLink.DanhengServer.Game.Mission.FinishType.Handler;

[MissionFinishType(MissionFinishTypeEnum.KillMonster)]
public class MissionHandlerKillMonster : MissionFinishTypeHandler
{
    public override async ValueTask HandleFinishType(PlayerInstance player, SubMissionInfo info, object? arg)
    {
        if (arg is not EntityMonster monster) return;
        if (monster.InstID == info.ParamInt2)
            if (!monster.IsAlive)
                await player.MissionManager!.FinishSubMission(info.ID);
    }
}