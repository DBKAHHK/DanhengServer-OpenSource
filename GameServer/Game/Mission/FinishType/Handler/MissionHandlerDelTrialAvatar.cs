using EggLink.DanhengServer.Data.Config;
using EggLink.DanhengServer.Enums;
using EggLink.DanhengServer.Game.Player;

namespace EggLink.DanhengServer.Game.Mission.FinishType.Handler;

[MissionFinishType(MissionFinishTypeEnum.DelTrialAvatar)]
public class MissionHandlerDelTrialAvatar : MissionFinishTypeHandler
{
    public override async ValueTask HandleFinishType(PlayerInstance player, SubMissionInfo info, object? arg)
    {
        await player.MissionManager!.FinishSubMission(info.ID);
    }
}