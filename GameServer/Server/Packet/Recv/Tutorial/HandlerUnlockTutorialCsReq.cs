using EggLink.DanhengServer.Database;
using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Server.Packet.Send.Tutorial;

namespace EggLink.DanhengServer.Server.Packet.Recv.Tutorial;

[Opcode(CmdIds.UnlockTutorialCsReq)]
public class HandlerUnlockTutorialCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = UnlockTutorialCsReq.Parser.ParseFrom(data);
        var player = connection.Player!;
        if (!player.TutorialData!.Tutorials.TryGetValue((int)req.TutorialId, out var _))
        {
            player.TutorialData!.Tutorials.Add((int)req.TutorialId, TutorialStatus.TutorialUnlock);
            DatabaseHelper.Instance?.UpdateInstance(player.TutorialData!);
        }

        await connection.SendPacket(new PacketUnlockTutorialScRsp(req.TutorialId));
    }
}