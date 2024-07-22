using EggLink.DanhengServer.Server.Packet.Send.Others;

namespace EggLink.DanhengServer.Server.Packet.Recv.Others;

[Opcode(CmdIds.GetVideoVersionKeyCsReq)]
public class HandlerGetVideoVersionKeyCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        await connection.SendPacket(new PacketGetVideoVersionKeyScRsp());
    }
}