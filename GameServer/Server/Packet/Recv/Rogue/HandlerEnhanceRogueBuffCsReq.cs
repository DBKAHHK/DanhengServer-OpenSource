using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Server.Packet.Send.Rogue;

namespace EggLink.DanhengServer.Server.Packet.Recv.Rogue;

[Opcode(CmdIds.EnhanceRogueBuffCsReq)]
public class HandlerEnhanceRogueBuffCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = EnhanceRogueBuffCsReq.Parser.ParseFrom(data);

        var rogue = connection.Player!.RogueManager?.GetRogueInstance();
        if (rogue == null) return;

        await rogue.EnhanceBuff((int)req.MazeBuffId, RogueActionSource.RogueCommonActionResultSourceTypeEnhance);

        await connection.SendPacket(new PacketEnhanceRogueBuffScRsp(req.MazeBuffId));
    }
}