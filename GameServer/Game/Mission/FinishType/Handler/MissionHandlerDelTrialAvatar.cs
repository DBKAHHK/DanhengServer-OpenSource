using EggLink.DanhengServer.Data.Config;
using EggLink.DanhengServer.Enums.Mission;
using EggLink.DanhengServer.GameServer.Game.Player;

namespace EggLink.DanhengServer.GameServer.Game.Mission.FinishType.Handler;

[MissionFinishType(MissionFinishTypeEnum.DelTrialAvatar)]
public class MissionHandlerDelTrialAvatar : MissionFinishTypeHandler
{
    public override async ValueTask HandleFinishType(PlayerInstance player, SubMissionInfo info, object? arg)
    {
        await player.MissionManager!.FinishSubMission(info.ID);
    }
}