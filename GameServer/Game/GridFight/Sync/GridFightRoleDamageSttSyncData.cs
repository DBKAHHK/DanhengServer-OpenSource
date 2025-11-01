using EggLink.DanhengServer.GameServer.Game.GridFight.Component;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Game.GridFight.Sync;

public class GridFightRoleDamageSttSyncData(GridFightSrc src, GridFightLevelComponent comp) : BaseGridFightSyncData(src)
{
    public override GridFightSyncData ToProto()
    {
        return new GridFightSyncData
        {
            GridFightDamageSttInfo = comp.ToDamageSttInfo()
        };
    }
}