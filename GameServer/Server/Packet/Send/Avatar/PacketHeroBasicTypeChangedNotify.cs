using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Avatar;

public class PacketHeroBasicTypeChangedNotify : BasePacket
{
    public PacketHeroBasicTypeChangedNotify(int type) : base(CmdIds.HeroBasicTypeChangedNotify)
    {
        var proto = new HeroBasicTypeChangedNotify
        {
            CurBasicType = (HeroBasicType)type
        };

        SetData(proto);
    }
}