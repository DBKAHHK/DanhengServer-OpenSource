using EggLink.DanhengServer.Data.Config;
using EggLink.DanhengServer.Enums;
using EggLink.DanhengServer.Game.Mission.FinishType;
using EggLink.DanhengServer.Game.Player;

namespace EggLink.DanhengServer.GameServer.Game.Mission.FinishType.Handler;

[MissionFinishType(MissionFinishTypeEnum.TeamLeaderChange)]
public class MissionHandlerTeamLeaderChange : MissionFinishTypeHandler
{
    public override async ValueTask HandleFinishType(PlayerInstance player, SubMissionInfo info, object? arg)
    {
        if (player.LineupManager!.GetCurLineup()!.LeaderAvatarId == info.ParamInt1)
            await player.MissionManager!.FinishSubMission(info.ID);
    }
}