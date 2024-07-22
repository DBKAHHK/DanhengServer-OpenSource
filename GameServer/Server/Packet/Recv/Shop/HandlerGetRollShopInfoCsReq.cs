using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Server.Packet.Send.Shop;

namespace EggLink.DanhengServer.Server.Packet.Recv.Shop;

[Opcode(CmdIds.GetRollShopInfoCsReq)]
public class HandlerGetRollShopInfoCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = GetRollShopInfoCsReq.Parser.ParseFrom(data);

        await connection.SendPacket(new PacketGetRollShopInfoScRsp(req.RollShopId));
    }
}