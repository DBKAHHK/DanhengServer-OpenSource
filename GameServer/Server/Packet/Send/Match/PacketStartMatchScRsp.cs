using EggLink.DanhengServer.GameServer.Game.MatchThree.Member;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Match;

public class PacketStartMatchScRsp : BasePacket
{
    public PacketStartMatchScRsp(MatchThreeMemberInstance instance) : base(CmdIds.StartMatchScRsp)
    {
        var proto = new StartMatchScRsp
        {
            LobbyExtraInfo = instance.ToExtraInfo()
        };

        SetData(proto);
    }
}