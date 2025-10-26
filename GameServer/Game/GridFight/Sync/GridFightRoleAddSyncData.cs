using EggLink.DanhengServer.GameServer.Game.GridFight.Component;
using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Proto.ServerSide;

namespace EggLink.DanhengServer.GameServer.Game.GridFight.Sync;

public class GridFightRoleAddSyncData(GridFightSrc src, GridFightRoleInfoPb role) : BaseGridFightSyncData(src)
{
    public override GridFightSyncData ToProto()
    {
        return new GridFightSyncData
        {
            AddRoleInfo = role.ToProto()
        };
    }
}