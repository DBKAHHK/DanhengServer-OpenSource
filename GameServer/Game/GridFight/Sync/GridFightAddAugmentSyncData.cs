using EggLink.DanhengServer.GameServer.Game.GridFight.Component;
using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Proto.ServerSide;

namespace EggLink.DanhengServer.GameServer.Game.GridFight.Sync;

public class GridFightAddAugmentSyncData(GridFightSrc src, GridFightGameAugmentPb augment, uint groupId = 0) : BaseGridFightSyncData(src, groupId)
{
    public override GridFightSyncData ToProto()
    {
        return new GridFightSyncData
        {
            AugmentSyncInfo = new GridFightAugmentSyncInfo
            {
                SyncAugmentInfo = augment.ToProto()
            }
        };
    }
}