using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Proto.ServerSide;

namespace EggLink.DanhengServer.GameServer.Game.GridFight.Sync;

public class GridFightMaxAvatarNumSyncData(GridFightSrc src, GridFightBasicInfoPb info) : BaseGridFightSyncData(src)
{
    public override GridFightSyncData ToProto()
    {
        return new GridFightSyncData
        {
            MaxBattleRoleNum = info.MaxAvatarNum
        };
    }
}