using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.Server.Packet.Recv.Rogue;

[Opcode(CmdIds.FinishRogueCommonDialogueCsReq)]
public class HandlerFinishRogueCommonDialogueCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = FinishRogueCommonDialogueCsReq.Parser.ParseFrom(data);

        var rogue = connection.Player!.RogueManager?.GetRogueInstance();
        if (rogue == null) return;

        await rogue.HandleFinishDialogueGroup((int)req.EventUniqueId);

        await connection.SendPacket(CmdIds.FinishRogueCommonDialogueScRsp);
    }
}