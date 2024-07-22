using EggLink.DanhengServer.Server.Packet.Send.Lineup;

namespace EggLink.DanhengServer.Server.Packet.Recv.Lineup;

[Opcode(CmdIds.GetLineupAvatarDataCsReq)]
public class HandlerGetLineupAvatarDataCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        await connection.SendPacket(new PacketGetLineupAvatarDataScRsp(connection.Player!));
    }
}