using EggLink.DanhengServer.Server.Packet.Send.ChessRogue;

namespace EggLink.DanhengServer.Server.Packet.Recv.ChessRogue;

[Opcode(CmdIds.GetChessRogueBuffEnhanceInfoCsReq)]
public class HandlerGetChessRogueBuffEnhanceInfoCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        await connection.SendPacket(new PacketGetChessRogueBuffEnhanceInfoScRsp(connection.Player!));
    }
}