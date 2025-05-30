using EggLink.DanhengServer.GameServer.Server.Packet.Send.EraFlipper;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.EraFlipper;

[Opcode(CmdIds.EnterEraFlipperRegionCsReq)]
public class HandlerEnterEraFlipperRegionCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = EnterEraFlipperRegionCsReq.Parser.ParseFrom(data);

        await connection.SendPacket(new PacketEnterEraFlipperRegionScRsp(req.EraFlipperRegionId));
    }
}