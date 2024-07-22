using EggLink.DanhengServer.Game.Player;

namespace EggLink.DanhengServer.Server.Packet.Send.Gacha;

public class PacketGetGachaInfoScRsp : BasePacket
{
    public PacketGetGachaInfoScRsp(PlayerInstance player) : base(CmdIds.GetGachaInfoScRsp)
    {
        SetData(player.GachaManager!.ToProto());
    }
}