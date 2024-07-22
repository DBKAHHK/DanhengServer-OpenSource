using EggLink.DanhengServer.Common.Enums;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.Scene;
using EggLink.DanhengServer.Server.Packet.Send.Player;

namespace EggLink.DanhengServer.Server.Packet.Recv.Player;

[Opcode(CmdIds.PlayerLoginCsReq)]
public class HandlerPlayerLoginCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        connection.State = SessionStateEnum.ACTIVE;
        await connection.SendPacket(new PacketPlayerLoginScRsp(connection));
        await connection.SendPacket(new PacketContentPackageSyncDataScNotify());
    }
}