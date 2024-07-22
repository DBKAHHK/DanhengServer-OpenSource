using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Server.Packet.Send.ChessRogue;

namespace EggLink.DanhengServer.Server.Packet.Recv.ChessRogue;

[Opcode(CmdIds.ChessRogueNousEditDiceCsReq)]
public class HandlerChessRogueNousEditDiceCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var player = connection.Player!;
        var req = ChessRogueNousEditDiceCsReq.Parser.ParseFrom(data);

        var diceData = player.ChessRogueManager!.SetDice(req.DiceInfo);

        await connection.SendPacket(new PacketChessRogueNousEditDiceScRsp(diceData));
    }
}