using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.EraFlipperData;
public class PacketEraFlipperDataChangeScNotify : BasePacket
{
    public PacketEraFlipperDataChangeScNotify(ChangeEraFlipperDataCsReq req, int floorId) : base(CmdIds.EraFlipperDataChangeScNotify)
    {
        var proto = new EraFlipperDataChangeScNotify
        {
            Data = req.Data,
            FloorId = (uint)floorId
        };

        SetData(proto);
    }
}
