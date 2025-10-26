using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Game.GridFight.Sync;

public abstract class BaseGridFightSyncData(GridFightSrc src)
{
    public GridFightSrc Src { get; set; } = src;
    public abstract GridFightSyncData ToProto();
}