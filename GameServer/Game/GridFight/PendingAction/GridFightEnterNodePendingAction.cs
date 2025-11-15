using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Game.GridFight.PendingAction;

public class GridFightEnterNodePendingAction(GridFightInstance inst) : BaseGridFightPendingAction(inst)
{
    public override GridFightPendingAction ToProto()
    {
        return new GridFightPendingAction
        {
            EnterNodeAction = new GridFightEnterNodeActionInfo(),
            QueuePosition = QueuePosition
        };
    }
}