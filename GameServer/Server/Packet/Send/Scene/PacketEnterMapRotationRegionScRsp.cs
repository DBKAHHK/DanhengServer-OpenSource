using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.Server.Packet.Send.Scene;

public class PacketEnterMapRotationRegionScRsp : BasePacket
{
    public PacketEnterMapRotationRegionScRsp(MotionInfo motion) : base(CmdIds.EnterMapRotationRegionScRsp)
    {
        var proto = new EnterMapRotationRegionScRsp
        {
            Motion = motion,
            EnergyInfo = new RotatorEnergyInfo
            {
                CurNum = 5,
                MaxNum = 5
            }
        };

        SetData(proto);
    }
}