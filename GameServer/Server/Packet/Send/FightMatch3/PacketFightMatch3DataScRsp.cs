using EggLink.DanhengServer.GameServer.Game.MatchThree;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.FightMatch3;

public class PacketFightMatch3DataScRsp : BasePacket
{
    public PacketFightMatch3DataScRsp(MatchThreeGameInstance inst, int uid) : base(CmdIds.FightMatch3DataScRsp)
    {
        var proto = new FightMatch3DataScRsp
        {
            Data = inst.ToProto(uid),
            MemberInfo = { inst.Members.Select(x => x.ToProto()) }
        };
        SetData(proto);
    }
}