using EggLink.DanhengServer.Data.Config;
using EggLink.DanhengServer.Enums.Mission;
using EggLink.DanhengServer.GameServer.Game.Player;

namespace EggLink.DanhengServer.GameServer.Game.Mission.FinishType.Handler;

[MissionFinishType(MissionFinishTypeEnum.FinishMission)]
public class MissionHandlerFinishMission : MissionFinishTypeHandler
{
    public override async ValueTask HandleFinishType(PlayerInstance player, SubMissionInfo info, object? arg)
    {
        var send = true;
        foreach (var mainMissionId in info.ParamIntList ?? [])
            if (player.MissionManager!.GetMainMissionStatus(mainMissionId) != MissionPhaseEnum.Finish)
            {
                send = false;
                break;
            }

        if (send) await player.MissionManager!.FinishSubMission(info.ID);
    }
}