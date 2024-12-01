using EggLink.DanhengServer.GameServer.Game.MatchThree;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Match;

public class PacketMatchResultScNotify : BasePacket
{
    public PacketMatchResultScNotify(MatchThreeGameInstance instance) : base(CmdIds.MatchResultScNotify)
    {
        var proto = new MatchResultScNotify
        {
            MemberInfo = { instance.Members.Select(x => x.ToProto()) }
        };

        SetData(proto);
    }
}