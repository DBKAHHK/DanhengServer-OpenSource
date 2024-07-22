using EggLink.DanhengServer.Enums;
using EggLink.DanhengServer.Game.Player;

namespace EggLink.DanhengServer.Game.Mission.FinishAction.Handler;

[MissionFinishAction(FinishActionTypeEnum.delMission)]
public class MissionHandlerDelMission : MissionFinishActionHandler
{
    public override async ValueTask OnHandle(List<int> Params, List<string> ParamString, PlayerInstance Player)
    {
        if (Params.Count < 1) return;
        var missionId = Params[0];
        await Player.MissionManager!.FinishSubMission(missionId);
    }
}