using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.Internationalization;
using EggLink.DanhengServer.Server.Packet.Send.Player;

namespace EggLink.DanhengServer.Command.Cmd;

[CommandInfo("avatar", "Game.Command.Avatar.Desc", "Game.Command.Avatar.Usage")]
public class CommandAvatar : ICommand
{
    [CommandMethod("talent")]
    public async ValueTask SetTalent(CommandArg arg)
    {
        if (arg.Target == null)
        {
            await arg.SendMsg(I18nManager.Translate("Game.Command.Notice.PlayerNotFound"));
            return;
        }

        if (arg.BasicArgs.Count < 2)
        {
            await arg.SendMsg(I18nManager.Translate("Game.Command.Notice.InvalidArguments"));
            return;
        }

        // change basic type
        var avatarId = arg.GetInt(0);
        var level = arg.GetInt(1);
        if (level < 0 || level > 10)
        {
            await arg.SendMsg(I18nManager.Translate("Game.Command.Avatar.InvalidLevel",
                I18nManager.Translate("Word.Talent")));
            return;
        }

        var player = arg.Target.Player!;
        if (avatarId == -1)
        {
            player.AvatarManager!.AvatarData.Avatars.ForEach(avatar =>
            {
                if (avatar.HeroId > 0)
                {
                    avatar.SkillTreeExtra.TryGetValue(avatar.HeroId, out var hero);
                    hero ??= [];
                    var excel = GameData.AvatarConfigData[avatar.HeroId];
                    excel.SkillTree.ForEach(talent => { hero[talent.PointID] = Math.Min(level, talent.MaxLevel); });
                }
                else
                {
                    avatar.Excel?.SkillTree.ForEach(talent =>
                    {
                        avatar.SkillTree[talent.PointID] = Math.Min(level, talent.MaxLevel);
                    });
                }
            });
            await arg.SendMsg(I18nManager.Translate("Game.Command.Avatar.AllAvatarsLevelSet",
                I18nManager.Translate("Word.Talent"), level.ToString()));

            // sync
            await player.SendPacket(new PacketPlayerSyncScNotify(player.AvatarManager.AvatarData.Avatars));

            return;
        }

        var avatar = player.AvatarManager!.GetAvatar(avatarId);
        if (avatar == null)
        {
            await arg.SendMsg(I18nManager.Translate("Game.Command.Avatar.AvatarNotFound"));
            return;
        }

        avatar.Excel?.SkillTree.ForEach(talent =>
        {
            avatar.SkillTree[talent.PointID] = Math.Min(level, talent.MaxLevel);
        });

        // sync
        await player.SendPacket(new PacketPlayerSyncScNotify(avatar));

        await arg.SendMsg(I18nManager.Translate("Game.Command.Avatar.AvatarLevelSet",
            avatar.Excel?.Name?.Replace("{NICKNAME}", player.Data.Name) ?? avatarId.ToString(),
            I18nManager.Translate("Word.Talent"), level.ToString()));
    }

    [CommandMethod("get")]
    public async ValueTask GetAvatar(CommandArg arg)
    {
        if (arg.Target == null)
        {
            await arg.SendMsg(I18nManager.Translate("Game.Command.Notice.PlayerNotFound"));
            return;
        }

        if (arg.BasicArgs.Count < 1) await arg.SendMsg(I18nManager.Translate("Game.Command.Notice.InvalidArguments"));

        var id = arg.GetInt(0);
        var excel = await arg.Target.Player!.AvatarManager!.AddAvatar(id);

        if (excel == null)
        {
            await arg.SendMsg(I18nManager.Translate("Game.Command.Avatar.AvatarFailedGet", id.ToString()));
            return;
        }

        await arg.SendMsg(I18nManager.Translate("Game.Command.Avatar.AvatarGet", excel.Name ?? id.ToString()));
    }

    [CommandMethod("rank")]
    public async ValueTask SetRank(CommandArg arg)
    {
        if (arg.Target == null)
        {
            await arg.SendMsg(I18nManager.Translate("Game.Command.Notice.PlayerNotFound"));
            return;
        }

        if (arg.BasicArgs.Count < 2) await arg.SendMsg(I18nManager.Translate("Game.Command.Notice.InvalidArguments"));

        var id = arg.GetInt(0);
        var rank = arg.GetInt(1);
        if (rank < 0 || rank > 6)
        {
            await arg.SendMsg(I18nManager.Translate("Game.Command.Avatar.InvalidLevel",
                I18nManager.Translate("Word.Rank")));
            return;
        }

        if (id == -1)
        {
            arg.Target.Player!.AvatarManager!.AvatarData.Avatars.ForEach(avatar =>
            {
                avatar.Rank = Math.Min(rank, 6);
            });
            await arg.SendMsg(I18nManager.Translate("Game.Command.Avatar.AllAvatarsLevelSet",
                I18nManager.Translate("Word.Rank"), rank.ToString()));

            // sync
            await arg.Target.SendPacket(
                new PacketPlayerSyncScNotify(arg.Target.Player!.AvatarManager.AvatarData.Avatars));
        }
        else
        {
            var avatar = arg.Target.Player!.AvatarManager!.GetAvatar(id);
            if (avatar == null)
            {
                await arg.SendMsg(I18nManager.Translate("Game.Command.Avatar.AvatarNotFound"));
                return;
            }

            avatar.Rank = Math.Min(rank, 6);

            // sync
            await arg.Target.SendPacket(new PacketPlayerSyncScNotify(avatar));

            await arg.SendMsg(I18nManager.Translate("Game.Command.Avatar.AvatarLevelSet",
                avatar.Excel?.Name?.Replace("{NICKNAME}", arg.Target.Player!.Data.Name) ?? id.ToString(),
                I18nManager.Translate("Word.Rank"), rank.ToString()));
        }
    }

    [CommandMethod("level")]
    public async ValueTask SetLevel(CommandArg arg)
    {
        if (arg.Target == null)
        {
            await arg.SendMsg(I18nManager.Translate("Game.Command.Notice.PlayerNotFound"));
            return;
        }

        if (arg.BasicArgs.Count < 2)
        {
            await arg.SendMsg(I18nManager.Translate("Game.Command.Notice.InvalidArguments"));
            return;
        }

        var id = arg.GetInt(0);
        var level = arg.GetInt(1);
        if (level < 1 || level > 80)
        {
            await arg.SendMsg(I18nManager.Translate("Game.Command.Avatar.InvalidLevel",
                I18nManager.Translate("Word.Avatar")));
            return;
        }

        if (id == -1)
        {
            arg.Target.Player!.AvatarManager!.AvatarData.Avatars.ForEach(avatar =>
            {
                avatar.Level = Math.Min(level, 80);
                avatar.Promotion = GameData.GetMinPromotionForLevel(avatar.Level);
            });
            await arg.SendMsg(I18nManager.Translate("Game.Command.Avatar.AllAvatarsLevelSet",
                I18nManager.Translate("Word.Avatar"), level.ToString()));

            // sync
            await arg.Target.SendPacket(
                new PacketPlayerSyncScNotify(arg.Target.Player!.AvatarManager.AvatarData.Avatars));
        }
        else
        {
            var avatar = arg.Target.Player!.AvatarManager!.GetAvatar(id);
            if (avatar == null)
            {
                await arg.SendMsg(I18nManager.Translate("Game.Command.Avatar.AvatarNotFound"));
                return;
            }

            avatar.Level = Math.Min(level, 80);
            avatar.Promotion = GameData.GetMinPromotionForLevel(avatar.Level);

            // sync
            await arg.Target.SendPacket(new PacketPlayerSyncScNotify(avatar));

            await arg.SendMsg(I18nManager.Translate("Game.Command.Avatar.AvatarLevelSet",
                avatar.Excel?.Name?.Replace("{NICKNAME}", arg.Target.Player!.Data.Name) ?? id.ToString(),
                I18nManager.Translate("Word.Avatar"), level.ToString()));
        }
    }
}