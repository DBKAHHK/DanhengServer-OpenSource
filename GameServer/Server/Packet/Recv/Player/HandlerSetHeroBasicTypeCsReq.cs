using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Server.Packet.Send.Player;

namespace EggLink.DanhengServer.Server.Packet.Recv.Player;

[Opcode(CmdIds.SetHeroBasicTypeCsReq)]
public class HandlerSetHeroBasicTypeCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = SetHeroBasicTypeCsReq.Parser.ParseFrom(data);

        var player = connection.Player!;
        var avatar = player.AvatarManager!.GetHero();
        if (avatar == null)
        {
            await connection.SendPacket(new PacketSetHeroBasicTypeScRsp());
            return;
        }

        avatar.HeroId = (int)req.BasicType;
        player.Data.CurBasicType = (int)req.BasicType;

        await connection.SendPacket(new PacketPlayerSyncScNotify(avatar));
        await connection.SendPacket(new PacketSetHeroBasicTypeScRsp((uint)req.BasicType));
    }
}