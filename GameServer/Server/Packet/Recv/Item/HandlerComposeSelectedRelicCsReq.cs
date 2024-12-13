using EggLink.DanhengServer.GameServer.Server.Packet.Send.Item;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.Item;

[Opcode(CmdIds.ComposeSelectedRelicCsReq)]
public class HandlerComposeSelectedRelicCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = ComposeSelectedRelicCsReq.Parser.ParseFrom(data);
        var player = connection.Player!;
        var item = await player.InventoryManager!.ComposeRelic(req);
        if (item == null)
        {
            await connection.SendPacket(new PacketComposeSelectedRelicScRsp());
            return;
        }

        await connection.SendPacket(new PacketComposeSelectedRelicScRsp(req.ComposeId, item));
    }
}