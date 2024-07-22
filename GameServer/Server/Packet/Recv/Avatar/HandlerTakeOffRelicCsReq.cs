using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.Server.Packet.Recv.Avatar;

[Opcode(CmdIds.TakeOffRelicCsReq)]
public class HandlerTakeOffRelicCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = TakeOffRelicCsReq.Parser.ParseFrom(data);
        foreach (var param in req.RelicTypeList)
            await connection.Player!.InventoryManager!.UnequipRelic((int)req.DressAvatarId, (int)param);

        await connection.SendPacket(CmdIds.TakeOffRelicScRsp);
    }
}