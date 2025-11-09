using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Game.GridFight.PendingAction;

public class GridFightPreparePendingAction(GridFightInstance inst) : BaseGridFightPendingAction(inst)
{
    public override GridFightPendingAction ToProto()
    {
        return new GridFightPendingAction
        {
            PrepareAction = new GridFightPrepareActionInfo(),
            QueuePosition = QueuePosition
        };
    }
}