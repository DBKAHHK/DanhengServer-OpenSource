using EggLink.DanhengServer.GameServer.Game.Challenge.Instances;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.ChallengePeak;

public class PacketChallengePeakSettleScNotify : BasePacket
{
    public PacketChallengePeakSettleScNotify(ChallengePeakInstance inst) : base(CmdIds.ChallengePeakSettleScNotify)
    {
        var proto = new ChallengePeakSettleScNotify
        {
            PeakStar = inst.Data.Peak.Stars,
            IsWin = inst.IsWin,
            PeakLevelId = inst.Data.Peak.CurrentPeakLevelId
        };

        SetData(proto);
    }
}