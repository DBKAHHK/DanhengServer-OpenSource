namespace EggLink.DanhengServer.Server.Packet.Recv.Friend;

[Opcode(CmdIds.GetChatFriendHistoryCsReq)]
public class HandlerGetChatFriendHistoryCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        await connection.SendPacket(CmdIds.GetChatFriendHistoryScRsp);
    }
}