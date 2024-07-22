using EggLink.DanhengServer.Server.Packet.Send.Rogue;

namespace EggLink.DanhengServer.Server.Packet.Recv.Rogue;

[Opcode(CmdIds.QuitRogueCsReq)]
public class HandlerQuitRogueCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        if (connection.Player!.RogueManager?.RogueInstance == null) return;
        await connection.Player!.RogueManager!.RogueInstance.QuitRogue();
        await connection.SendPacket(new PacketQuitRogueScRsp(connection.Player!));
    }
}