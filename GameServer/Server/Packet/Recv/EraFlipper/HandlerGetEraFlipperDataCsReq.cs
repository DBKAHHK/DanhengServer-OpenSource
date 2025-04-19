using EggLink.DanhengServer.GameServer.Server.Packet.Send.EraFlipper;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.EraFlipper;

[Opcode(CmdIds.GetEraFlipperDataCsReq)]
public class HandlerGetEraFlipperDataCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = GetEraFlipperDataCsReq.Parser.ParseFrom(data);

        await connection.SendPacket(new PacketGetEraFlipperDataScRsp(connection.Player!));
    }
}
