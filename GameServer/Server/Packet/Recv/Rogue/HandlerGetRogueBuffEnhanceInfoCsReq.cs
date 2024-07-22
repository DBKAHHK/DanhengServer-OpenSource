using EggLink.DanhengServer.Server.Packet.Send.Rogue;

namespace EggLink.DanhengServer.Server.Packet.Recv.Rogue;

[Opcode(CmdIds.GetRogueBuffEnhanceInfoCsReq)]
public class HandlerGetRogueBuffEnhanceInfoCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        await connection.SendPacket(new PacketGetRogueBuffEnhanceInfoScRsp(connection.Player!));
    }
}