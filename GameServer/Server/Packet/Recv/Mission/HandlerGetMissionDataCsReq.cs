using EggLink.DanhengServer.Server.Packet.Send.Mission;

namespace EggLink.DanhengServer.Server.Packet.Recv.Mission;

[Opcode(CmdIds.GetMissionDataCsReq)]
public class HandlerGetMissionDataCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        await connection.SendPacket(new PacketGetMissionDataScRsp(connection.Player!));
    }
}