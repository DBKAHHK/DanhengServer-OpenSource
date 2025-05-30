using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.EraFlipper;

public class PacketEnterEraFlipperRegionScRsp : BasePacket
{
    public PacketEnterEraFlipperRegionScRsp(uint regionId) : base(CmdIds.EnterEraFlipperRegionScRsp)
    {
        var proto = new EnterEraFlipperRegionScRsp
        {
            EraFlipperRegionId = regionId
        };

        SetData(proto);
    }
}