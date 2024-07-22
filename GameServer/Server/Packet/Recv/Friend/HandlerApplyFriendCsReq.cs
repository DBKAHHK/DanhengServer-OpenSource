using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Server.Packet.Send.Friend;

namespace EggLink.DanhengServer.Server.Packet.Recv.Friend;

[Opcode(CmdIds.ApplyFriendCsReq)]
public class HandlerApplyFriendCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = ApplyFriendCsReq.Parser.ParseFrom(data);

        await connection.Player!.FriendManager!.AddFriend((int)req.Uid);

        await connection.SendPacket(new PacketApplyFriendScRsp(req.Uid));
    }
}