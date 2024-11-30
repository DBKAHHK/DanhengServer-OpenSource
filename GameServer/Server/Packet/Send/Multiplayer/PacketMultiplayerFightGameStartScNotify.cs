using EggLink.DanhengServer.GameServer.Game.MatchThree;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Multiplayer;

public class PacketMultiplayerFightGameStartScNotify : BasePacket
{
    public PacketMultiplayerFightGameStartScNotify(MatchThreeGameInstance game) : base(CmdIds.MultiplayerFightGameStartScNotify)
    {
        var proto = new MultiplayerFightGameStartScNotify
        {
            MemberInfo = { game.Members.Select(x => x.ToProto()) },
            FightGameInfo = game.ToBasicInfo()
        };

        SetData(proto);
    }
}