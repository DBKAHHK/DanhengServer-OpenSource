using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Server.Packet.Send.Avatar;

namespace EggLink.DanhengServer.Server.Packet.Recv.Avatar;

[Opcode(CmdIds.SetDisplayAvatarCsReq)]
public class HandlerSetDisplayAvatarCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = SetDisplayAvatarCsReq.Parser.ParseFrom(data);
        var player = connection.Player!;
        var avatars = player.AvatarManager!.AvatarData!.DisplayAvatars;
        avatars.Clear();
        foreach (var id in req.DisplayAvatarList)
        {
            if (id.AvatarId == 0) continue;
            avatars.Add((int)id.AvatarId);
        }

        await connection.SendPacket(new PacketSetDisplayAvatarScRsp(req.DisplayAvatarList));
    }
}