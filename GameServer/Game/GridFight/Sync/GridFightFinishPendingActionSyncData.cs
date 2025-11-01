using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Game.GridFight.Sync;

public class GridFightFinishPendingActionSyncData(GridFightSrc src, uint queuePosition) : BaseGridFightSyncData(src)
{
    public override GridFightSyncData ToProto()
    {
        return new GridFightSyncData
        {
        };
    }
}