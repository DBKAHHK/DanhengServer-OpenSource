using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.Server.Packet.Send.Player;

public class PacketDeployRotaterScRsp : BasePacket
{
    public PacketDeployRotaterScRsp(RotaterData rotaterData, int curNum, int maxNum) : base(CmdIds.DeployRotaterScRsp)
    {
        var proto = new DeployRotaterScRsp
        {
            EnergyInfo = new RotatorEnergyInfo
            {
                MaxNum = (uint)maxNum,
                CurNum = (uint)curNum
            },
            RotaterData = rotaterData
        };

        SetData(proto);
    }
}