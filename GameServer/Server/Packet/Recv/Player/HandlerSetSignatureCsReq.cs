using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Server.Packet.Send.Player;

namespace EggLink.DanhengServer.Server.Packet.Recv.Player;

[Opcode(CmdIds.SetSignatureCsReq)]
public class HandlerSetSignatureCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = SetSignatureCsReq.Parser.ParseFrom(data);

        connection.Player!.Data.Signature = req.Signature;

        await connection.SendPacket(new PacketSetSignatureScRsp(req.Signature));
    }
}