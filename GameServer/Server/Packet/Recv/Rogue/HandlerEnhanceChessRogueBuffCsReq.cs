using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Server.Packet.Send.ChessRogue;

namespace EggLink.DanhengServer.Server.Packet.Recv.Rogue;

[Opcode(CmdIds.EnhanceChessRogueBuffCsReq)]
public class HandlerEnhanceChessRogueBuffCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = EnhanceChessRogueBuffCsReq.Parser.ParseFrom(data);

        await connection.Player!.ChessRogueManager!.RogueInstance!.EnhanceBuff((int)req.MazeBuffId,
            RogueActionSource.RogueCommonActionResultSourceTypeEnhance);
        await connection.SendPacket(
            new PacketEnhanceChessRogueBuffScRsp(connection.Player!.ChessRogueManager!.RogueInstance!, req.MazeBuffId));
    }
}