using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.Database.Avatar;
using EggLink.DanhengServer.Database.Inventory;
using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Util;
using SqlSugar;
using LineupInfo = EggLink.DanhengServer.Database.Lineup.LineupInfo;

namespace EggLink.DanhengServer.Database.Player;

[SugarTable("Player")]
public class PlayerData : BaseDatabaseDataHelper
{
    public string? Name { get; set; } = "";
    public string? Signature { get; set; } = "";
    public int Birthday { get; set; } = 0;
    public int CurBasicType { get; set; } = 8001;
    public int HeadIcon { get; set; } = 208001;
    public int PhoneTheme { get; set; } = 221000;
    public int ChatBubble { get; set; } = 220000;
    public int PersonalCard { get; set; } = 253000;
    public int PhoneCase { get; set; } = 254000;
    public int CurrentBgm { get; set; } = 210007;
    public int CurrentPamSkin { get; set; } = 252000;
    public bool IsGenderSet { get; set; } = false;
    public Gender CurrentGender { get; set; } = Gender.Man;
    public int Level { get; set; } = 1;
    public int Exp { get; set; } = 0;
    public int WorldLevel { get; set; } = 0;
    public int Scoin { get; set; } = 0; // Credits
    public int Hcoin { get; set; } = 0; // Jade
    public int Mcoin { get; set; } = 0; // Crystals
    public int TalentPoints { get; set; } = 0; // Rogue talent points

    public int Pet { get; set; } = 0;
    [SugarColumn(IsNullable = true)] public int CurMusicLevel { get; set; }

    public int Stamina { get; set; } = 300;
    public double StaminaReserve { get; set; } = 0;
    public long NextStaminaRecover { get; set; } = 0;

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
        var result = DatabaseHelper.Instance?.GetInstance<PlayerData>((int)uid);
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
        if (!GameData.ChatBubbleConfigData.ContainsKey(ChatBubble)) // to avoid npe
            ChatBubble = 220000;

        var instance = DatabaseHelper.Instance!.GetInstance<AvatarData>(Uid)!;

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
        foreach (var avatar in instance.AssistAvatars.Select(
                     assist => instance.FormalAvatars.Find(x => x.AvatarId == assist)!))
            info.AssistSimpleInfoList.Add(new AssistSimpleInfo
            {
                AvatarId = (uint)avatar.AvatarId,
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
        var inventoryInfo = DatabaseHelper.Instance!.GetInstance<InventoryData>(Uid);

        if (avatarInfo == null || inventoryInfo == null) return info;

        var pos = 0;
        foreach (var avatar in avatarInfo.AssistAvatars.Select(assist =>
                     avatarInfo.FormalAvatars.Find(x => x.AvatarId == assist)!))
            info.AssistAvatarList.Add(avatar.ToDetailProto(pos++, new PlayerDataCollection(this, inventoryInfo, new LineupInfo())));

        pos = 0;
        foreach (var avatar in avatarInfo.DisplayAvatars.Select(display =>
                     avatarInfo.FormalAvatars.Find(x => x.AvatarId == display)!))
            info.DisplayAvatarList.Add(avatar.ToDetailProto(pos++, new PlayerDataCollection(this, inventoryInfo, new LineupInfo())));

        return info;
    }
}