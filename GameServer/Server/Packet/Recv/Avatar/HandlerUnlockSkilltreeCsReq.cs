using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.Database;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.Avatar;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.Player;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.Avatar;

[Opcode(CmdIds.UnlockSkilltreeCsReq)]
public class HandlerUnlockSkilltreeCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = UnlockSkilltreeCsReq.Parser.ParseFrom(data);
        var player = connection.Player!;
        GameData.AvatarSkillTreeConfigData.TryGetValue((int)(req.PointId * 10 + req.Level), out var config);
        if (config == null)
        {
            await connection.SendPacket(new PacketUnlockSkilltreeScRsp());
            return;
        }

        var avatar = player.AvatarManager!.GetAvatar(config.AvatarID);
        if (avatar == null)
        {
            await connection.SendPacket(new PacketUnlockSkilltreeScRsp());
            return;
        }

        foreach (var cost in req.ItemList)
            await connection.Player!.InventoryManager!.RemoveItem((int)cost.PileItem.ItemId,
                (int)cost.PileItem.ItemNum);

        avatar.GetSkillTree().TryGetValue((int)req.PointId, out var level);
        avatar.GetSkillTree()[(int)req.PointId] = level + 1;
        DatabaseHelper.Instance!.UpdateInstance(player.AvatarManager.AvatarData!);

        await connection.SendPacket(new PacketPlayerSyncScNotify(avatar));
        await connection.SendPacket(new PacketUnlockSkilltreeScRsp(req.PointId, req.Level));
    }
}