using EggLink.DanhengServer.Server.Packet.Send.Lineup;

namespace EggLink.DanhengServer.Server.Packet.Recv.Lineup;

[Opcode(CmdIds.GetCurLineupDataCsReq)]
public class HandlerGetCurLineupDataCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        await connection.SendPacket(new PacketGetCurLineupDataScRsp(connection.Player!));
    }
}