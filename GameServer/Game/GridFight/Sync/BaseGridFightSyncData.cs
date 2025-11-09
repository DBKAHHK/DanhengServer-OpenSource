using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Game.GridFight.Sync;

public abstract class BaseGridFightSyncData(GridFightSrc src, uint groupId = 0)
{
    public GridFightSrc Src { get; set; } = src;
    public uint GroupId { get; set; } = groupId;
    public abstract GridFightSyncData ToProto();
}