using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Proto.ServerSide;

namespace EggLink.DanhengServer.GameServer.Game.GridFight.Sync;

public class GridFightPlayerLevelSyncData(GridFightSrc src, GridFightBasicInfoPb info) : BaseGridFightSyncData(src)
{
    public override GridFightSyncData ToProto()
    {
        return new GridFightSyncData
        {
            PlayerLevel = new GridFightPlayerLevelSyncInfo
            {
                Exp = info.LevelExp,
                Level = info.CurLevel
            }
        };
    }
}