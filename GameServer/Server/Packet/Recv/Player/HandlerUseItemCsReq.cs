using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.Server.Packet.Recv.Player;

[Opcode(CmdIds.UseItemCsReq)]
public class HandlerUseItemCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = UseItemCsReq.Parser.ParseFrom(data);

        await connection.SendPacket(CmdIds.UseItemScRsp);
    }
}