using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Server.Packet.Send.Player;

namespace EggLink.DanhengServer.Server.Packet.Recv.Player;

[Opcode(CmdIds.InteractChargerCsReq)]
public class HandlerInteractChargerCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = InteractChargerCsReq.Parser.ParseFrom(data);

        connection.Player!.ChargerNum = 5;
        await connection.SendPacket(new PacketInteractChargerScRsp(req.ChargerInfo));
        await connection.SendPacket(new PacketUpdateEnergyScNotify(connection.Player!.ChargerNum, 5));
    }
}