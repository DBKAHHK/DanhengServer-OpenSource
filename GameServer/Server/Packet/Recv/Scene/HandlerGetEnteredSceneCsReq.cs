using EggLink.DanhengServer.Server.Packet.Send.Scene;

namespace EggLink.DanhengServer.Server.Packet.Recv.Scene;

[Opcode(CmdIds.GetEnteredSceneCsReq)]
public class HandlerGetEnteredSceneCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        await connection.SendPacket(new PacketGetEnteredSceneScRsp());
    }
}