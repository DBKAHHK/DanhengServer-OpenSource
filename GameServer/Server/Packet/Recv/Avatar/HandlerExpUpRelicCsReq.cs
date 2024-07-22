using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Server.Packet.Send.Avatar;

namespace EggLink.DanhengServer.Server.Packet.Recv.Avatar;

[Opcode(CmdIds.ExpUpRelicCsReq)]
public class HandlerExpUpRelicCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = ExpUpRelicCsReq.Parser.ParseFrom(data);

        var left = await connection.Player!.InventoryManager!.LevelUpRelic((int)req.RelicUniqueId, req.CostData);

        await connection.SendPacket(new PacketExpUpRelicScRsp(left));
    }
}