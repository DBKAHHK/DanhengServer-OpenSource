using EggLink.DanhengServer.GameServer.Server.Packet.Send.Mission;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.Mission;

[Opcode(CmdIds.SelectInclinationTextCsReq)]
public class HandlerSelectInclinationTextCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = SelectInclinationTextCsReq.Parser.ParseFrom(data);

        await connection.SendPacket(new PacketSelectInclinationTextScRsp(req.TalkSentenceId));
    }
}