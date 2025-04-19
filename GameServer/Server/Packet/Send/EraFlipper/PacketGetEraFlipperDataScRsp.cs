using EggLink.DanhengServer.GameServer.Game.Player;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.EraFlipper;
public class PacketGetEraFlipperDataScRsp : BasePacket
{
    public PacketGetEraFlipperDataScRsp(PlayerInstance player) : base(CmdIds.GetEraFlipperDataScRsp)
    {
        var proto = new GetEraFlipperDataScRsp();

        SetData(proto);
    }
}
