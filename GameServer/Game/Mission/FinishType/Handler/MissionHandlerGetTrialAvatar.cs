using EggLink.DanhengServer.Data.Config;
using EggLink.DanhengServer.Enums;
using EggLink.DanhengServer.Game.Player;

namespace EggLink.DanhengServer.Game.Mission.FinishType.Handler;

[MissionFinishType(MissionFinishTypeEnum.GetTrialAvatar)]
public class MissionHandlerGetTrialAvatar : MissionFinishTypeHandler
{
    public override async ValueTask HandleFinishType(PlayerInstance player, SubMissionInfo info, object? arg)
    {
        await player.MissionManager!.FinishSubMission(info.ID);
    }
}