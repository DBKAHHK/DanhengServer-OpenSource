using EggLink.DanhengServer.Game.Rogue.Scene;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.Server.Packet.Send.Rogue;

public class PacketSyncRogueMapRoomScNotify : BasePacket
{
    public PacketSyncRogueMapRoomScNotify(RogueRoomInstance room, int mapId) : base(CmdIds.SyncRogueMapRoomScNotify)
    {
        var proto = new SyncRogueMapRoomScNotify
        {
            CurRoom = room.ToProto(),
            MapId = (uint)mapId
        };

        SetData(proto);
    }
}