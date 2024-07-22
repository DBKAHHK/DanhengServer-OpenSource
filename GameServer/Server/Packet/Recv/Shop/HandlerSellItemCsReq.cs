using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Server.Packet.Send.Shop;

namespace EggLink.DanhengServer.Server.Packet.Recv.Shop;

[Opcode(CmdIds.SellItemCsReq)]
public class HandlerSellItemCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = SellItemCsReq.Parser.ParseFrom(data);
        var items = await connection.Player!.InventoryManager!.SellItem(req.CostData);
        await connection.SendPacket(new PacketSellItemScRsp(items));
    }
}