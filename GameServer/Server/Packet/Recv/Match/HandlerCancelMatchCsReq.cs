using EggLink.DanhengServer.GameServer.Server.Packet.Send.Match;
using EggLink.DanhengServer.Kcp;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.Match;

[Opcode(CmdIds.CancelMatchCsReq)]
public class HandlerCancelMatchCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var player = connection.Player!;
        var room = player.MatchThreeManager!.RoomInstance;
        if (room == null) return;

        await room.CancelMatch();
        await connection.SendPacket(CmdIds.CancelMatchScRsp);
    }
}