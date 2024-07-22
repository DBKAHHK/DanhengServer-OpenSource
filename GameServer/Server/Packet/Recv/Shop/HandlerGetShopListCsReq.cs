using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Server.Packet.Send.Shop;

namespace EggLink.DanhengServer.Server.Packet.Recv.Shop;

[Opcode(CmdIds.GetShopListCsReq)]
public class HandlerGetShopListCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = GetShopListCsReq.Parser.ParseFrom(data);

        await connection.SendPacket(new PacketGetShopListScRsp(req.ShopType));
    }
}