using EggLink.DanhengServer.GameServer.Server.Packet.Send.ChessRogue;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.ChessRogue;

[Opcode(CmdIds.ChessRogueQueryCsReq)]
public class HandlerChessRogueQueryCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        await connection.SendPacket(new PacketChessRogueQueryScRsp(connection.Player!));
    }
}