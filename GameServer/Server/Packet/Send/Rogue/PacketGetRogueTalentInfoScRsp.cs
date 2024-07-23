using EggLink.DanhengServer.GameServer.Game.Rogue;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Rogue;

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