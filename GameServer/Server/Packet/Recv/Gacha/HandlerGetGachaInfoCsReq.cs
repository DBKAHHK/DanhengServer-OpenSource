using EggLink.DanhengServer.Server.Packet.Send.Gacha;

namespace EggLink.DanhengServer.Server.Packet.Recv.Gacha;

[Opcode(CmdIds.GetGachaInfoCsReq)]
public class HandlerGetGachaInfoCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        await connection.SendPacket(new PacketGetGachaInfoScRsp(connection.Player!));
    }
}