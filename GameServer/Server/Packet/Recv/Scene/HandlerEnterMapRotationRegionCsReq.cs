using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Server.Packet.Send.Scene;

namespace EggLink.DanhengServer.Server.Packet.Recv.Scene;

[Opcode(CmdIds.EnterMapRotationRegionCsReq)]
public class HandlerEnterMapRotationRegionCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = EnterMapRotationRegionCsReq.Parser.ParseFrom(data);
        await connection.SendPacket(new PacketEnterMapRotationRegionScRsp(req.Motion));
    }
}