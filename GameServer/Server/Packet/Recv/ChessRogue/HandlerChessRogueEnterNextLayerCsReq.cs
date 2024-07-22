namespace EggLink.DanhengServer.Server.Packet.Recv.ChessRogue;

[Opcode(CmdIds.ChessRogueEnterNextLayerCsReq)]
public class HandlerChessRogueEnterNextLayerCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        await connection.Player!.ChessRogueManager!.RogueInstance!.EnterNextLayer();
    }
}