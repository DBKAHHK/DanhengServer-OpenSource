using EggLink.DanhengServer.Game.Player;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.Server.Packet.Send.Player;

public class PacketSetHeadIconScRsp : BasePacket
{
    public PacketSetHeadIconScRsp(PlayerInstance player) : base(CmdIds.SetHeadIconScRsp)
    {
        var proto = new SetHeadIconScRsp
        {
            CurrentHeadIconId = (uint)player.Data.HeadIcon
        };
        SetData(proto);
    }
}