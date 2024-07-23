using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.Database.Inventory;
using EggLink.DanhengServer.Enums.Item;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.Player;
using EggLink.DanhengServer.Internationalization;

namespace EggLink.DanhengServer.Command.Command.Cmd;

[CommandInfo("giveall", "Game.Command.GiveAll.Desc", "Game.Command.GiveAll.Usage")]
public class CommandGiveall : ICommand
{
    [CommandMethod("0 avatar")]
    public async ValueTask GiveAllAvatar(CommandArg arg)
    {
        var player = arg.Target?.Player;
        if (player == null)
        {
            await arg.SendMsg(I18nManager.Translate("Game.Command.Notice.PlayerNotFound"));
            return;
        }

        arg.CharacterArgs.TryGetValue("r", out var rankStr);
        arg.CharacterArgs.TryGetValue("l", out var levelStr);
        rankStr ??= "1";
        levelStr ??= "1";
        if (!int.TryParse(rankStr, out var rank) || !int.TryParse(levelStr, out var level))
        {
            await arg.SendMsg(I18nManager.Translate("Game.Command.Notice.InvalidArguments"));
            return;
        }

        var avatarList = GameData.AvatarConfigData.Values;
        foreach (var avatar in avatarList)
            if (player.AvatarManager!.GetAvatar(avatar.AvatarID) == null)
            {
                await player.InventoryManager!.AddItem(avatar.AvatarID, 1, false, sync: false);
                player.AvatarManager!.GetAvatar(avatar.AvatarID)!.Level = Math.Max(Math.Min(level, 80), 0);
                player.AvatarManager!.GetAvatar(avatar.AvatarID)!.Promotion =
                    GameData.GetMinPromotionForLevel(Math.Max(Math.Min(level, 80), 0));
                player.AvatarManager!.GetAvatar(avatar.AvatarID)!.Rank = Math.Max(Math.Min(rank, 6), 0);
            }
            else
            {
                player.AvatarManager!.GetAvatar(avatar.AvatarID)!.Level = Math.Max(Math.Min(level, 80), 0);
                player.AvatarManager!.GetAvatar(avatar.AvatarID)!.Promotion =
                    GameData.GetMinPromotionForLevel(Math.Max(Math.Min(level, 80), 0));
                player.AvatarManager!.GetAvatar(avatar.AvatarID)!.Rank = Math.Max(Math.Min(rank, 6), 0);
            }

        await player.SendPacket(new PacketPlayerSyncScNotify(player.AvatarManager!.AvatarData.Avatars));

        await arg.SendMsg(I18nManager.Translate("Game.Command.GiveAll.GiveAllItems",
            I18nManager.Translate("Word.Avatar"), "1"));
    }

    [CommandMethod("0 equipment")]
    public async ValueTask GiveAllLightcone(CommandArg arg)
    {
        var player = arg.Target?.Player;
        if (player == null)
        {
            await arg.SendMsg(I18nManager.Translate("Game.Command.Notice.PlayerNotFound"));
            return;
        }

        arg.CharacterArgs.TryGetValue("r", out var rankStr);
        arg.CharacterArgs.TryGetValue("l", out var levelStr);
        arg.CharacterArgs.TryGetValue("x", out var amountStr);
        rankStr ??= "1";
        levelStr ??= "1";
        amountStr ??= "1";
        if (!int.TryParse(rankStr, out var rank) || !int.TryParse(levelStr, out var level) ||
            !int.TryParse(amountStr, out var amount))
        {
            await arg.SendMsg(I18nManager.Translate("Game.Command.Notice.InvalidArguments"));
            return;
        }

        var lightconeList = GameData.EquipmentConfigData.Values;
        var items = new List<ItemData>();

        for (var i = 0; i < amount; i++)
            foreach (var lightcone in lightconeList)
            {
                var item = await player.InventoryManager!.AddItem(lightcone.EquipmentID, 1, false,
                    Math.Max(Math.Min(rank, 5), 0), Math.Max(Math.Min(level, 80), 0), false);

                if (item != null)
                    items.Add(item);
            }

        await player.SendPacket(new PacketPlayerSyncScNotify(items));

        await arg.SendMsg(I18nManager.Translate("Game.Command.GiveAll.GiveAllItems",
            I18nManager.Translate("Word.Equipment"), amount.ToString()));
    }

    [CommandMethod("0 material")]
    public async ValueTask GiveAllMaterial(CommandArg arg)
    {
        var player = arg.Target?.Player;
        if (player == null)
        {
            await arg.SendMsg(I18nManager.Translate("Game.Command.Notice.PlayerNotFound"));
            return;
        }

        arg.CharacterArgs.TryGetValue("x", out var amountStr);
        amountStr ??= "1";
        if (!int.TryParse(amountStr, out var amount))
        {
            await arg.SendMsg(I18nManager.Translate("Game.Command.Notice.InvalidArguments"));
            return;
        }

        var materialList = GameData.ItemConfigData.Values;
        var items = new List<ItemData>();
        foreach (var material in materialList)
            if (material.ItemMainType == ItemMainTypeEnum.Material)
                items.Add(new ItemData
                {
                    ItemId = material.ID,
                    Count = amount
                });

        await player.InventoryManager!.AddItems(items, false);
        await arg.SendMsg(I18nManager.Translate("Game.Command.GiveAll.GiveAllItems",
            I18nManager.Translate("Word.Material"), amount.ToString()));
    }

    [CommandMethod("0 relic")]
    public async ValueTask GiveAllRelic(CommandArg arg)
    {
        var player = arg.Target?.Player;
        if (player == null)
        {
            await arg.SendMsg(I18nManager.Translate("Game.Command.Notice.PlayerNotFound"));
            return;
        }

        arg.CharacterArgs.TryGetValue("l", out var levelStr);
        levelStr ??= "1";
        if (!int.TryParse(levelStr, out var level))
        {
            await arg.SendMsg(I18nManager.Translate("Game.Command.Notice.InvalidArguments"));
            return;
        }

        arg.CharacterArgs.TryGetValue("x", out var amountStr);
        amountStr ??= "1";
        if (!int.TryParse(amountStr, out var amount))
        {
            await arg.SendMsg(I18nManager.Translate("Game.Command.Notice.InvalidArguments"));
            return;
        }

        var relicList = GameData.RelicConfigData.Values;
        var items = new List<ItemData>();

        for (var i = 0; i < amount; i++)
            foreach (var relic in relicList)
            {
                var item = await player.InventoryManager!.AddItem(relic.ID, amount, false, 1,
                    Math.Max(Math.Min(level, relic.MaxLevel), 1), false);

                if (item != null)
                    items.Add(item);
            }

        await player.SendPacket(new PacketPlayerSyncScNotify(items));

        await arg.SendMsg(I18nManager.Translate("Game.Command.GiveAll.GiveAllItems",
            I18nManager.Translate("Word.Relic"), amount.ToString()));
    }

    [CommandMethod("0 unlock")]
    public async ValueTask GiveAllUnlock(CommandArg arg)
    {
        var player = arg.Target?.Player;
        if (player == null)
        {
            await arg.SendMsg(I18nManager.Translate("Game.Command.Notice.PlayerNotFound"));
            return;
        }

        var materialList = GameData.ItemConfigData.Values;
        foreach (var material in materialList)
            if (material.ItemMainType == ItemMainTypeEnum.Usable)
                if (material.ItemSubType == ItemSubTypeEnum.HeadIcon ||
                    material.ItemSubType == ItemSubTypeEnum.PhoneTheme ||
                    material.ItemSubType == ItemSubTypeEnum.ChatBubble)
                    await player.InventoryManager!.AddItem(material.ID, 1, false);

        await arg.SendMsg(I18nManager.Translate("Game.Command.GiveAll.GiveAllItems",
            I18nManager.Translate("Word.Unlock"), "1"));
    }
}