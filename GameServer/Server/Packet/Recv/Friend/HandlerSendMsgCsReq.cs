using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.Server.Packet.Recv.Friend;

[Opcode(CmdIds.SendMsgCsReq)]
public class HandlerSendMsgCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = SendMsgCsReq.Parser.ParseFrom(data);

        await connection.SendPacket(CmdIds.SendMsgScRsp);

        if (req.MessageType == MsgType.CustomText)
            await connection.Player!.FriendManager!.SendMessage(connection.Player!.Uid, (int)req.TargetList[0],
                req.MessageText);
        else if (req.MessageType == MsgType.Emoji)
            await connection.Player!.FriendManager!.SendMessage(connection.Player!.Uid, (int)req.TargetList[0], null,
                (int)req.ExtraId);
    }
}