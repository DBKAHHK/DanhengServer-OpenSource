using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Proto.ServerSide;

namespace EggLink.DanhengServer.GameServer.Game.GridFight.Sync;

public class GridFightRoleRemoveSyncData(GridFightSrc src, GridFightRoleInfoPb role, uint groupId = 0) : BaseGridFightSyncData(src, groupId)
{
    public override GridFightSyncData ToProto()
    {
        return new GridFightSyncData
        {
            RemoveRoleUniqueId = role.UniqueId
        };
    }
}