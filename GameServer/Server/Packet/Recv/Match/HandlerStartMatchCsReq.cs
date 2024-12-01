using EggLink.DanhengServer.GameServer.Server.Packet.Send.Match;
using EggLink.DanhengServer.Kcp;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.Match;

[Opcode(CmdIds.StartMatchCsReq)]
public class HandlerStartMatchCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var player = connection.Player!;
        var room = player.MatchThreeManager!.RoomInstance;
        if (room == null) return;

        await room.StartMatch();
        // get member instance
        var member = room.GetMemberByUid(player.Uid);

        // send packet
        if (member == null) return;
        await connection.SendPacket(new PacketStartMatchScRsp(member));
    }
}