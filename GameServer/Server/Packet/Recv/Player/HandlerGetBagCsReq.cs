using EggLink.DanhengServer.Server.Packet.Send.Player;

namespace EggLink.DanhengServer.Server.Packet.Recv.Player;

[Opcode(CmdIds.GetBagCsReq)]
public class HandlerGetBagCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        await connection.SendPacket(new PacketGetBagScRsp(connection.Player!));
    }
}