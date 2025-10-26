using EggLink.DanhengServer.GameServer.Game.GridFight.Component;
using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Proto.ServerSide;

namespace EggLink.DanhengServer.GameServer.Game.GridFight.Sync;

public class GridFightShopSyncData(GridFightSrc src, GridFightShopInfoPb data, uint level) : BaseGridFightSyncData(src)
{
    public override GridFightSyncData ToProto()
    {
        return new GridFightSyncData
        {
            ShopSyncInfo = data.ToSyncInfo(level)
        };
    }
}