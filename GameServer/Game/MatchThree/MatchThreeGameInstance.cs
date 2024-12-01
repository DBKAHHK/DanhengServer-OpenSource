using EggLink.DanhengServer.GameServer.Game.MatchThree.Member;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.FightMatch3;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.Multiplayer;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Game.MatchThree;

public class MatchThreeGameInstance(FightGameMode mode)
{
    public List<MatchThreeMemberInstance> Members { get; set; } = [];
    public FightGameMode GameMode { get; set; } = mode;
    public Match3State State { get; set; } = Match3State.Game;

    public async ValueTask SyncMatchedResult()
    {
        foreach (var member in Members)
        {
            await member.Player.SendPacket(new PacketMultiplayerFightGameStartScNotify(this));
        }
    }

    public async ValueTask TurnStart()
    {
        foreach (var member in Members)
        {
            member.Opponent = member;
            await member.Player.SendPacket(new PacketFightMatch3TurnStartScNotify(this, member.Player.Uid));
        }
    }

    public MultiplayerFightGameInfo ToBasicInfo()
    {
        return new MultiplayerFightGameInfo
        {
            GameMode = FightGameMode.Match3
        };
    }

    public FightMatch3Data ToProto(int uid)
    {
        return new FightMatch3Data
        {
            Match3State = State,
            PPJLNEDNDAH = 1,
            PlayerInfoList = { Members.Select(x => x.ToPlayerInfo()) },
            Match3CurInfo = Members.Find(x => x.Player.Uid == uid)!.ToCurPlayerInfo()
        };
    }
}