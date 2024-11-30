using EggLink.DanhengServer.GameServer.Game.MatchThree;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Lobby;

public class PacketLobbySyncInfoScNotify : BasePacket
{
    public PacketLobbySyncInfoScNotify(MatchThreeRoomInstance room, int uid, LobbyModifyType modifyType) : base(CmdIds.LobbySyncInfoScNotify)
    {
        var proto = new LobbySyncInfoScNotify
        {
            MemberInfo = { room.Members.Select(x => x.ToProto()) },
            Type = modifyType,
            Uid = (uint)uid
        };

        SetData(proto);
    }
}