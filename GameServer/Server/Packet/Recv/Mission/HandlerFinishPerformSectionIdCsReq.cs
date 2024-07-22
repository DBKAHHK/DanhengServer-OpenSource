using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Server.Packet.Send.Mission;

namespace EggLink.DanhengServer.Server.Packet.Recv.Mission;

[Opcode(CmdIds.FinishPerformSectionIdCsReq)]
public class HandlerFinishPerformSectionIdCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = FinishPerformSectionIdCsReq.Parser.ParseFrom(data);

        await connection.Player!.MessageManager!.FinishSection((int)req.SectionId);

        await connection.SendPacket(new PacketFinishPerformSectionIdScRsp(req.SectionId));
    }
}