using EggLink.DanhengServer.GameServer.Server.Packet.Send.Scene;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.Server.Packet.Recv.Scene;

[Opcode(CmdIds.EnterSceneCsReq)]
public class HandlerEnterSceneCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = EnterSceneCsReq.Parser.ParseFrom(data);
        var overMapTp = await connection.Player!.EnterScene((int)req.EntryId, (int)req.TeleportId, true,
            storyLineId: (int)req.GameStoryLineId, mapTp: req.MapTp);

        await connection.SendPacket(new PacketEnterSceneScRsp(overMapTp, req.MapTp, (int)req.GameStoryLineId));
    }
}