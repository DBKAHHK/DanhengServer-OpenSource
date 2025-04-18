using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.Database.Avatar;
using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Util;
using SqlSugar;

namespace EggLink.DanhengServer.Database.Player;

[SugarTable("Player")]
public class PlayerData : BaseDatabaseDataHelper
{
    public string? Name { get; set; } = "";
    public string? Signature { get; set; } = "";
    public int Birthday { get; set; } = 0;
    public int HeadIcon { get; set; } = 208001;
    public int PhoneTheme { get; set; } = 221000;
    public int ChatBubble { get; set; } = 220000;
    public int PersonalCard { get; set; } = 253000;
    public int PhoneCase { get; set; } = 254000;
    public int CurrentBgm { get; set; } = 210007;
    public int CurrentPamSkin { get; set; } = 252000;
    public int Pet { get; set; } = 0;
    public bool IsGenderSet { get; set; } = false;
    public Gender CurrentGender { get; set; }
    public int Level { get; set; } = 1;
    public int Exp { get; set; } = 0;
    public int WorldLevel { get; set; } = 0;
    public int Scoin { get; set; } = 0; // Credits
    public int Hcoin { get; set; } = 0; // Jade
    public int Mcoin { get; set; } = 0; // Crystals
    public int TalentPoints { get; set; } = 0; // Rogue talent points

    [SugarColumn(IsNullable = true)] public int CurMusicLevel { get; set; }

    public int Stamina { get; set; } = 300;
    public double StaminaReserve { get; set; } = 0;
    public long NextStaminaRecover { get; set; } = 0;
    public long MonthCard { get; set; } = 0;

    [SugarColumn(IsJson = true)] public List<int> AssistAvatars { get; set; } = [];
    [SugarColumn(IsJson = true)] public List<int> DisplayAvatars { get; set; } = [];

    [SugarColumn(IsNullable = true, IsJson = true)]
    public Position? Pos { get; set; }

    [SugarColumn(IsNullable = true, IsJson = true)]
    public Position? Rot { get; set; }

    [SugarColumn(IsNullable = true)] public int PlaneId { get; set; }

    [SugarColumn(IsNullable = true)] public int FloorId { get; set; }

    [SugarColumn(IsNullable = true)] public int EntryId { get; set; }

    [SugarColumn(IsNullable = true)] public long LastActiveTime { get; set; }

    [SugarColumn(IsJson = true)] public List<int> TakenLevelReward { get; set; } = [];

    public static PlayerData? GetPlayerByUid(long uid)
    {
        var result = DatabaseHelper.Instance!.GetInstance<PlayerData>((int)uid);
        return result;
    }

    public PlayerBasicInfo ToProto()
    {
        return new PlayerBasicInfo
        {
            Nickname = Name,
            Level = (uint)Level,
            Exp = (uint)Exp,
            WorldLevel = (uint)WorldLevel,
            Scoin = (uint)Scoin,
            Hcoin = (uint)Hcoin,
            Mcoin = (uint)Mcoin,
            Stamina = (uint)Stamina
        };
    }

    public PlayerSimpleInfo ToSimpleProto(FriendOnlineStatus status)
    {
        var info = new PlayerSimpleInfo
        {
            Nickname = Name,
            Level = (uint)Level,
            Signature = Signature,
            Uid = (uint)Uid,
            OnlineStatus = status,
            HeadIcon = (uint)HeadIcon,
            Platform = PlatformType.Pc,
            LastActiveTime = LastActiveTime,
            ChatBubbleId = (uint)ChatBubble,
            PersonalCard = (uint)PersonalCard
        };

        var pos = 0;
        var avatarData = DatabaseHelper.Instance!.GetInstance<AvatarData>(Uid)!;
        foreach (var avatar in AssistAvatars.Select(
            assist => avatarData.Avatars.Find(x => x.BaseAvatarId == assist)!))
            info.AssistSimpleInfoList.Add(new AssistSimpleInfo
            {
                AvatarId = (uint)avatar.BaseAvatarId,
                Level = (uint)avatar.Level,
                Pos = (uint)pos++
            });

        return info;
    }

    public PlayerDetailInfo ToDetailProto()
    {
        var info = new PlayerDetailInfo
        {
            Nickname = Name,
            Level = (uint)Level,
            Signature = Signature,
            IsBanned = false,
            HeadIcon = (uint)HeadIcon,
            PersonalCard = (uint)PersonalCard,
            Platform = PlatformType.Pc,
            Uid = (uint)Uid,
            WorldLevel = (uint)WorldLevel,
            RecordInfo = new PlayerRecordInfo()
        };

        var avatarInfo = DatabaseHelper.Instance!.GetInstance<AvatarData>(Uid);
        if (avatarInfo == null) return info;

        var pos = 0;
        foreach (var avatar in AssistAvatars.Select(assist =>
                     avatarInfo.Avatars.Find(x => x.BaseAvatarId == assist)!))
            info.AssistAvatarList.Add(avatar.ToDetailProto(Uid, pos++));

        pos = 0;
        foreach (var avatar in DisplayAvatars.Select(display =>
                     avatarInfo.Avatars.Find(x => x.BaseAvatarId == display)!))
            info.DisplayAvatarList.Add(avatar.ToDetailProto(Uid, pos++));

        return info;
    }
}