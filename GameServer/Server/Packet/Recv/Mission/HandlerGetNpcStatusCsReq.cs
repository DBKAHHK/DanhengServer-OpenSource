using EggLink.DanhengServer.GameServer.Server.Packet.Send.Mission;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.Mission;

[Opcode(CmdIds.GetNpcStatusCsReq)]
public class HandlerGetNpcStatusCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        await connection.SendPacket(new PacketGetNpcStatusScRsp(connection.Player!));
    }
}