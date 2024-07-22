using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.Server.Packet.Recv.Avatar;

[Opcode(CmdIds.DressAvatarCsReq)]
public class HandlerDressAvatarCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = DressAvatarCsReq.Parser.ParseFrom(data);
        var player = connection.Player!;

        await player.InventoryManager!.EquipAvatar((int)req.DressAvatarId, (int)req.EquipmentUniqueId);

        await connection.SendPacket(CmdIds.DressAvatarScRsp);
    }
}