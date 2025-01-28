using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.Data.Excel;
using EggLink.DanhengServer.Database;
using EggLink.DanhengServer.Database.Avatar;
using EggLink.DanhengServer.GameServer.Game.Player;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.Avatar;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.PlayerSync;
using EggLink.DanhengServer.Util;

namespace EggLink.DanhengServer.GameServer.Game.Avatar;

public class AvatarManager(PlayerInstance player) : BasePlayerManager(player)
{
    public AvatarData AvatarData { get; } = DatabaseHelper.Instance!.GetInstanceOrCreateNew<AvatarData>(player.Uid);

    public async ValueTask<AvatarConfigExcel?> AddAvatar(int avatarId, bool sync = true, bool notify = true,
        bool isGacha = false)
    {
        if (avatarId > 8000 && avatarId % 2 != (int)Player.Data.CurrentGender % 2) return null;
        GameData.AvatarConfigData.TryGetValue(avatarId, out var avatarExcel);
        if (avatarExcel == null) return null;

        GameData.MultiplePathAvatarConfigData.TryGetValue(avatarId, out var multiPathAvatar);
        var baseAvatarId = multiPathAvatar?.BaseAvatarID ?? avatarId;
        var avatarData = GetAvatar(baseAvatarId);

        var pathInfo = new MultiPathData();
        foreach (var skillTree in avatarExcel.DefaultSkillTree)
            pathInfo.SkillTree.Add(skillTree.PointID, 1);

        // Check if base avatar exist
        if (avatarData != null)
        {
            if (multiPathAvatar == null) return null;
            if (avatarData.PathInfo.TryAdd(avatarId, pathInfo))
                return avatarExcel;
            return null;
        }

        var avatar = new AvatarInfo(baseAvatarId, avatarId)
        {
            Level = 1,
            Timestamp = Extensions.GetUnixSec(),
            CurrentHp = 10000,
            CurrentSp = 0
        };
        avatar.PathInfo[avatarId] = pathInfo; // Add curAvatarId's pathinfo
        AvatarData.Avatars.Add(avatar);

        if (sync) await Player.SendPacket(new PacketPlayerSyncScNotify(avatar));
        if (notify) await Player.SendPacket(new PacketAddAvatarScNotify(avatar.BaseAvatarId, isGacha));

        return avatarExcel;
    }

    public AvatarInfo? GetAvatar(int avatarId)
    {
        if (GameData.MultiplePathAvatarConfigData.TryGetValue(avatarId, out var pathConfig))
            avatarId = pathConfig.BaseAvatarID;

        return AvatarData.Avatars.Find(avatar => avatar.BaseAvatarId == avatarId);
    }

}