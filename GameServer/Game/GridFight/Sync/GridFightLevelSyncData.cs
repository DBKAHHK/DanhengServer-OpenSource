using EggLink.DanhengServer.GameServer.Game.GridFight.Component;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Game.GridFight.Sync;

public class GridFightLevelSyncData(GridFightSrc src, GridFightLevelComponent level) : BaseGridFightSyncData(src)
{
    public override GridFightSyncData ToProto()
    {
        return new GridFightSyncData
        {
            LevelSyncInfo = new GridFightLevelSyncInfo
            {
                SectionId = level.CurrentSection.SectionId,
                ChapterId = level.CurrentSection.ChapterId,
                GridFightLayerInfo = new GridFightLayerInfo
                {
                    RouteInfo = level.CurrentSection.ToRouteInfo()
                }
            }
        };
    }
}