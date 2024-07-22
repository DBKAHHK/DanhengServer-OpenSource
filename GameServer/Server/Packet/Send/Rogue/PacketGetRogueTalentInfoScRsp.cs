using EggLink.DanhengServer.Game.Rogue;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.Server.Packet.Send.Rogue;

public class PacketGetRogueTalentInfoScRsp : BasePacket
{
    public PacketGetRogueTalentInfoScRsp() : base(CmdIds.GetRogueTalentInfoScRsp)
    {
        var proto = new GetRogueTalentInfoScRsp
        {
            RogueTalentInfo = RogueManager.ToTalentProto()
        };

        SetData(proto);
    }
}