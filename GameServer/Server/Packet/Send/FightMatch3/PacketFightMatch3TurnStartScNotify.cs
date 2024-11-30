using EggLink.DanhengServer.GameServer.Game.MatchThree;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.FightMatch3;

public class PacketFightMatch3TurnStartScNotify : BasePacket
{
    public PacketFightMatch3TurnStartScNotify(MatchThreeGameInstance inst, int uid) : base(CmdIds.FightMatch3TurnStartScNotify)
    {
        var proto = new FightMatch3TurnStartScNotify
        {
            LAOHPKGKKGO = inst.ToProto(uid),
        };
        SetData(proto);
    }
}