using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Server.Packet.Send.Player;

namespace EggLink.DanhengServer.Server.Packet.Recv.Player;

[Opcode(CmdIds.DeployRotaterCsReq)]
public class HandlerDeployRotaterCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = DeployRotaterCsReq.Parser.ParseFrom(data);

        connection.Player!.ChargerNum--;
        await connection.SendPacket(new PacketDeployRotaterScRsp(req.RotaterData, connection.Player!.ChargerNum, 5));
    }
}