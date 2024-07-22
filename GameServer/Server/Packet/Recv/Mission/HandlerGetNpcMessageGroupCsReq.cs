using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Server.Packet.Send.Mission;

namespace EggLink.DanhengServer.Server.Packet.Recv.Mission;

[Opcode(CmdIds.GetNpcMessageGroupCsReq)]
public class HandlerGetNpcMessageGroupCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = GetNpcMessageGroupCsReq.Parser.ParseFrom(data);

        await connection.SendPacket(new PacketGetNpcMessageGroupScRsp(req.ContactIdList, connection.Player!));
    }
}