using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.PlayerSync;
using EggLink.DanhengServer.Internationalization;

namespace EggLink.DanhengServer.Command.Command.Cmd;

[CommandInfo("avatar", "Game.Command.Avatar.Desc", "Game.Command.Avatar.Usage", ["av", "ava"])]
public class CommandAvatar : ICommand
{
    [CommandMethod("talent")]
    public async ValueTask SetTalent(CommandArg arg)
    {
        if (arg.Target == null)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.PlayerNotFound"));
            return;
        }

        if (arg.BasicArgs.Count < 2)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.InvalidArguments"));
            return;
        }

        var avatarId = arg.GetInt(0);
        var level = arg.GetInt(1);
        if (level is < 0 or > 10)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Avatar.InvalidLevel",
                I18NManager.Translate("Word.Talent")));
            return;
        }

        var player = arg.Target.Player!;
        if (avatarId == -1)
        {
            player.AvatarManager!.AvatarData.Avatars.ForEach(avatar =>
            {
                avatar.PathInfo.ToList().ForEach(path =>
                {
                    var avatarExcel = GameData.AvatarConfigData.Values.FirstOrDefault(x =>
                        x.AvatarID == path.Key)!;
                    foreach (var skillConfig in avatarExcel.SkillTree)
                        path.Value.SkillTree[skillConfig.PointID] = Math.Min(level, skillConfig.MaxLevel);
                });
            });
            await arg.SendMsg(I18NManager.Translate("Game.Command.Avatar.AllAvatarsLevelSet",
                I18NManager.Translate("Word.Talent"), level.ToString()));

            // sync
            await player.SendPacket(new PacketPlayerSyncScNotify(player.AvatarManager.AvatarData.Avatars));
        }
        else
        {
            var avatar = player.AvatarManager!.GetAvatar(avatarId);
            if (avatar == null)
            {
                await arg.SendMsg(I18NManager.Translate("Game.Command.Avatar.AvatarNotFound"));
                return;
            }
            var pathInfo = avatar.GetPathInfo(avatarId);
            if (pathInfo == null)
            {
                await arg.SendMsg(I18NManager.Translate("Game.Command.Avatar.AvatarNotFound"));
                return;
            }

            var avatarExcel = GameData.AvatarConfigData.Values.FirstOrDefault(x => x.AvatarID == avatarId)!;
            foreach (var skillTree in avatarExcel.SkillTree)
                pathInfo.SkillTree[skillTree.PointID] = Math.Min(level, skillTree.MaxLevel);

            // sync
            await player.SendPacket(new PacketPlayerSyncScNotify(avatar));

            await arg.SendMsg(I18NManager.Translate("Game.Command.Avatar.AvatarLevelSet",
                avatarExcel.Name?.Replace("{NICKNAME}", player.Data.Name) ?? avatarId.ToString(),
                I18NManager.Translate("Word.Talent"), level.ToString()));
        }
    }

    [CommandMethod("get")]
    public async ValueTask GetAvatar(CommandArg arg)
    {
        if (arg.Target == null)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.PlayerNotFound"));
            return;
        }

        if (arg.BasicArgs.Count < 1) await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.InvalidArguments"));

        var id = arg.GetInt(0);
        var excel = await arg.Target.Player!.AvatarManager!.AddAvatar(id);

        if (excel == null)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Avatar.AvatarFailedGet", id.ToString()));
            return;
        }

        await arg.SendMsg(I18NManager.Translate("Game.Command.Avatar.AvatarGet", excel.Name ?? id.ToString()));
    }

    [CommandMethod("rank")]
    public async ValueTask SetRank(CommandArg arg)
    {
        if (arg.Target == null)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.PlayerNotFound"));
            return;
        }

        if (arg.BasicArgs.Count < 2) await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.InvalidArguments"));

        var id = arg.GetInt(0);
        var rank = arg.GetInt(1);
        if (rank is < 0 or > 6)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Avatar.InvalidLevel",
                I18NManager.Translate("Word.Rank")));
            return;
        }

        if (id == -1)
        {
            arg.Target.Player!.AvatarManager!.AvatarData.Avatars.ForEach(avatar =>
                avatar.PathInfo.Values.ToList().ForEach(x => x.Rank = Math.Min(rank, 6))
            );
            await arg.SendMsg(I18NManager.Translate("Game.Command.Avatar.AllAvatarsLevelSet",
                I18NManager.Translate("Word.Rank"), rank.ToString()));

            // sync
            await arg.Target.SendPacket(
                new PacketPlayerSyncScNotify(arg.Target.Player!.AvatarManager.AvatarData.Avatars));
        }
        else
        {
            var avatar = arg.Target.Player!.AvatarManager!.GetAvatar(id);
            if (avatar == null)
            {
                await arg.SendMsg(I18NManager.Translate("Game.Command.Avatar.AvatarNotFound"));
                return;
            }
            var pathInfo = avatar.GetPathInfo(id);
            if (pathInfo == null)
            {
                await arg.SendMsg(I18NManager.Translate("Game.Command.Avatar.AvatarNotFound"));
                return;
            }

            var avatarExcel = GameData.AvatarConfigData.Values.FirstOrDefault(x => x.AvatarID == id)!;
            pathInfo.Rank = Math.Min(rank, avatarExcel.MaxRank);

            // sync
            await arg.Target.SendPacket(new PacketPlayerSyncScNotify(avatar));

            await arg.SendMsg(I18NManager.Translate("Game.Command.Avatar.AvatarLevelSet",
                avatarExcel.Name?.Replace("{NICKNAME}", arg.Target.Player!.Data.Name) ?? id.ToString(),
                I18NManager.Translate("Word.Rank"), rank.ToString()));
        }
    }

    [CommandMethod("level")]
    public async ValueTask SetLevel(CommandArg arg)
    {
        if (arg.Target == null)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.PlayerNotFound"));
            return;
        }

        if (arg.BasicArgs.Count < 2)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.InvalidArguments"));
            return;
        }

        var id = arg.GetInt(0);
        var level = arg.GetInt(1);
        if (level is < 1 or > 80)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Avatar.InvalidLevel",
                I18NManager.Translate("Word.Avatar")));
            return;
        }

        if (id == -1)
        {
            arg.Target.Player!.AvatarManager!.AvatarData.Avatars.ForEach(avatar =>
            {
                avatar.Level = Math.Min(level, 80);
                avatar.Promotion = GameData.GetMinPromotionForLevel(avatar.Level);
            });
            await arg.SendMsg(I18NManager.Translate("Game.Command.Avatar.AllAvatarsLevelSet",
                I18NManager.Translate("Word.Avatar"), level.ToString()));

            // sync
            await arg.Target.SendPacket(
                new PacketPlayerSyncScNotify(arg.Target.Player!.AvatarManager.AvatarData.Avatars));
        }
        else
        {
            var avatar = arg.Target.Player!.AvatarManager!.GetAvatar(id);
            if (avatar == null)
            {
                await arg.SendMsg(I18NManager.Translate("Game.Command.Avatar.AvatarNotFound"));
                return;
            }

            var avatarExcel = GameData.AvatarConfigData.Values.FirstOrDefault(x => x.AvatarID == id)!;
            avatar.Level = Math.Min(level, 80);
            avatar.Promotion = GameData.GetMinPromotionForLevel(avatar.Level);

            // sync
            await arg.Target.SendPacket(new PacketPlayerSyncScNotify(avatar));

            await arg.SendMsg(I18NManager.Translate("Game.Command.Avatar.AvatarLevelSet",
                avatarExcel.Name?.Replace("{NICKNAME}", arg.Target.Player!.Data.Name) ?? id.ToString(),
                I18NManager.Translate("Word.Avatar"), level.ToString()));
        }
    }
}