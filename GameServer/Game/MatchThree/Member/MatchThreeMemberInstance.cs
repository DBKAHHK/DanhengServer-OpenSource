using EggLink.DanhengServer.GameServer.Game.Player;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Game.MatchThree.Member;

public class MatchThreeMemberInstance(PlayerInstance player, int birdId, LobbyCharacterType type, MatchThreeRoomInstance room)
{
    public MatchThreeBirdInstance Bird { get; set; } = new()
    {
        BirdId = birdId
    };

    public LobbyCharacterType CharacterType { get; set; } = type;
    public LobbyCharacterStatus Status { get; set; } = LobbyCharacterStatus.Idle;
    public PlayerInstance Player { get; set; } = player;
    public MatchThreeRoomInstance RoomInstance { get; set; } = room;

    public int Score { get; set; }
    public Match3PlayerState State { get; set; } = Match3PlayerState.Alive;
    public MatchThreeMemberInstance? Opponent { get; set; }

    public MemberInfo ToProto()
    {
        return new MemberInfo
        {
            BasicInfo = ToMemberInfo(),
            StageInfo = ToExtraInfo(),
            StatusInfo = ToStatusInfo()
        };
    }

    public FightMatch3PlayerInfo ToPlayerInfo()
    {
        return new FightMatch3PlayerInfo
        {
            ScoreId = (uint)Score,
            State = State,
            Hp = (uint)Bird.Hp,
            GCCIOHEJPNE = (uint)Bird.BirdId,
            OOGAPOKFKAI = (uint)Player.Uid,
            Rank = 2,
            OpponentUid = (uint)(Opponent?.Player.Uid ?? 0)
        };
    }

    public FightMatch3CurInfo ToCurPlayerInfo()
    {
        return new FightMatch3CurInfo
        {
            ScoreId = (uint)Score,
            CurHp = (uint)Bird.Hp,
            IMKELKMHOIK = new DHPIFKICOPP(),
            CurPlayerState = State
        };
    }

    public MemberData ToMemberInfo()
    {
        return new MemberData
        {
            Nickname = Player.Data.Name,
            Uid = (uint)Player.Uid,
            Level = (uint)Player.Data.Level,
            HeadiconId = (uint)Player.Data.HeadIcon,
        };
    }

    public PlayerExtraInfo ToExtraInfo()
    {
        return new PlayerExtraInfo
        {
            GameBirdInfo = Bird.ToProto(),
            IsInMatch = RoomInstance.InMatch
        };
    }

    public PlayerStatusInfo ToStatusInfo()
    {
        return new PlayerStatusInfo
        {
            Status = Status,
            CharacterType = CharacterType
        };
    }
}