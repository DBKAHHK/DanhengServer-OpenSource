using EggLink.DanhengServer.Server.Packet.Send.Avatar;

namespace EggLink.DanhengServer.Server.Packet.Recv.Avatar;

[Opcode(CmdIds.GetAssistListCsReq)]
public class HandlerGetAssistListCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        await connection.SendPacket(new PacketGetAssistListScRsp());
    }
}