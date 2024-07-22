using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.Server.Packet.Send.Lineup;

public class PacketSceneCastSkillCostMpScRsp : BasePacket
{
    public PacketSceneCastSkillCostMpScRsp(int entityId) : base(CmdIds.SceneCastSkillCostMpScRsp)
    {
        var proto = new SceneCastSkillCostMpScRsp
        {
            CastEntityId = (uint)entityId
        };

        SetData(proto);
    }
}