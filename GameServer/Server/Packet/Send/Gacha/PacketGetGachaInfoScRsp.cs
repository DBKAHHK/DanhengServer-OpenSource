using EggLink.DanhengServer.GameServer.Game.Player;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Gacha;

public class PacketGetGachaInfoScRsp : BasePacket
{
    public PacketGetGachaInfoScRsp(PlayerInstance player) : base(CmdIds.GetGachaInfoScRsp)
    {
        SetData(player.GachaManager!.ToProto());
    }
}