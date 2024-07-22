using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Server.Packet.Send.Friend;

namespace EggLink.DanhengServer.Server.Packet.Recv.Friend;

[Opcode(CmdIds.GetPrivateChatHistoryCsReq)]
public class HandlerGetPrivateChatHistoryCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = GetPrivateChatHistoryCsReq.Parser.ParseFrom(data);

        await connection.SendPacket(new PacketGetPrivateChatHistoryScRsp(req.ContactId, connection.Player!));
    }
}