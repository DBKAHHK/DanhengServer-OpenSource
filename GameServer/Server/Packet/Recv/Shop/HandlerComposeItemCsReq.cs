using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Server.Packet.Send.Shop;

namespace EggLink.DanhengServer.Server.Packet.Recv.Shop;

[Opcode(CmdIds.ComposeItemCsReq)]
public class HandlerComposeItemCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = ComposeItemCsReq.Parser.ParseFrom(data);
        var player = connection.Player!;
        var item = await player.InventoryManager!.ComposeItem((int)req.ComposeId, (int)req.Count);
        if (item == null)
        {
            await connection.SendPacket(new PacketComposeItemScRsp());
            return;
        }

        await connection.SendPacket(new PacketComposeItemScRsp(req.ComposeId, req.Count, item));
    }
}