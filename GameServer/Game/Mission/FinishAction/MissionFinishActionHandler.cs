using EggLink.DanhengServer.Game.Player;

namespace EggLink.DanhengServer.Game.Mission.FinishAction;

public abstract class MissionFinishActionHandler
{
    public abstract ValueTask OnHandle(List<int> Params, List<string> ParamString, PlayerInstance Player);
}