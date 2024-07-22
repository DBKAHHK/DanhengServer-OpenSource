using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.Server.Packet.Send.Tutorial;

public class PacketUnlockTutorialGuideScRsp : BasePacket
{
    public PacketUnlockTutorialGuideScRsp(uint tutorialId) : base(CmdIds.UnlockTutorialGuideScRsp)
    {
        var proto = new UnlockTutorialGuideScRsp
        {
            TutorialGuide = new TutorialGuide
            {
                Id = tutorialId,
                Status = TutorialStatus.TutorialUnlock
            }
        };
        SetData(proto);
    }
}