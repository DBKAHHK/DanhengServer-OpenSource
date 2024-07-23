using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Lineup;

public class PacketSceneCastSkillMpUpdateScNotify : BasePacket
{
    public PacketSceneCastSkillMpUpdateScNotify(uint castEntityId, int mpCount) : base(
        CmdIds.SceneCastSkillMpUpdateScNotify)
    {
        var proto = new SceneCastSkillMpUpdateScNotify
        {
            CastEntityId = castEntityId,
            Mp = (uint)mpCount
        };

        SetData(proto);
    }
}