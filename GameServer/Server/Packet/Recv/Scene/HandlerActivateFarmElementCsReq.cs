using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Server.Packet.Send.Scene;

namespace EggLink.DanhengServer.Server.Packet.Recv.Scene;

[Opcode(CmdIds.ActivateFarmElementCsReq)]
public class HandlerActivateFarmElementCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = ActivateFarmElementCsReq.Parser.ParseFrom(data);

        await connection.SendPacket(new PacketActivateFarmElementScRsp(req.EntityId, connection.Player!));
    }
}