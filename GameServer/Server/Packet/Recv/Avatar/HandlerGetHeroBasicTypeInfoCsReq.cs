using EggLink.DanhengServer.GameServer.Server.Packet.Send.Avatar;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.Avatar;

[Opcode(CmdIds.GetHeroBasicTypeInfoCsReq)]
public class HandlerGetHeroBasicTypeInfoCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        await connection.SendPacket(new PacketGetHeroBasicTypeInfoScRsp(connection.Player!));
    }
}