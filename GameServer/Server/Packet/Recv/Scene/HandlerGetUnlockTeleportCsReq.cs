using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Server.Packet.Send.Scene;

namespace EggLink.DanhengServer.Server.Packet.Recv.Scene;

[Opcode(CmdIds.GetUnlockTeleportCsReq)]
public class HandlerGetUnlockTeleportCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = GetUnlockTeleportCsReq.Parser.ParseFrom(data);

        await connection.SendPacket(new PacketGetUnlockTeleportScRsp(req));
    }
}