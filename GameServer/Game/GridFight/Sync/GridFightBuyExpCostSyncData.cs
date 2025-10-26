using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Proto.ServerSide;

namespace EggLink.DanhengServer.GameServer.Game.GridFight.Sync;

public class GridFightBuyExpCostSyncData(GridFightSrc src, GridFightBasicInfoPb info) : BaseGridFightSyncData(src)
{
    public override GridFightSyncData ToProto()
    {
        return new GridFightSyncData
        {
            GridFightLevelCost = info.BuyLevelCost
        };
    }
}