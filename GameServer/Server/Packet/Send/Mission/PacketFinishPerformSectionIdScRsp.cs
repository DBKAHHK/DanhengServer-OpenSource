using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.Server.Packet.Send.Mission;

public class PacketFinishPerformSectionIdScRsp : BasePacket
{
    public PacketFinishPerformSectionIdScRsp(uint sectionId) : base(CmdIds.FinishPerformSectionIdScRsp)
    {
        var proto = new FinishPerformSectionIdScRsp
        {
            SectionId = sectionId
        };

        SetData(proto);
    }
}