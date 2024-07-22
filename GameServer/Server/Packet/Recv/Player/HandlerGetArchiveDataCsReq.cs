using EggLink.DanhengServer.Server.Packet.Send.Player;

namespace EggLink.DanhengServer.Server.Packet.Recv.Player;

[Opcode(CmdIds.GetArchiveDataCsReq)]
public class HandlerGetArchiveDataCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        await connection.SendPacket(new PacketGetArchiveDataScRsp());
    }
}