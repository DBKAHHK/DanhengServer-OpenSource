using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.Database.Inventory;
using EggLink.DanhengServer.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EggLink.DanhengServer.Command.Cmd
{
    [CommandInfo("relic", "给予玩家指定词条的遗器", "/relic <遗器ID> <主词条ID> <小词条ID1:小词条等级> <小词条ID2:小词条等级> <小词条ID3:小词条等级> <小词条ID4:小词条等级> l<等级> x<数量>")]
    public class CommandRelic : ICommand
    {
        [CommandDefault]
        public void GiveRelic(CommandArg arg)
        {
            if (arg.Target == null)
            {
                arg.SendMsg("未指定玩家");
                return;
            }

            var player = arg.Target.Player;
            if (player == null)
            {
                arg.SendMsg("玩家不存在");
                return;
            }

            if (arg.BasicArgs.Count < 3)
            {
                arg.SendMsg("无效选项");
                return;
            }

            arg.CharacterArgs.TryGetValue("x", out var str);
            arg.CharacterArgs.TryGetValue("l", out var levelStr);
            str ??= "1";
            levelStr ??= "1";
            if (!int.TryParse(str, out var amount) || !int.TryParse(levelStr, out var level))
            {
                arg.SendMsg("参数无效");
                return;
            }

            GameData.RelicConfigData.TryGetValue(int.Parse(arg.BasicArgs[0]), out var itemConfig);
            if (itemConfig == null)
            {
                arg.SendMsg("找不到物品");
                return;
            }

            GameData.RelicSubAffixData.TryGetValue(itemConfig.SubAffixGroup, out var subAffixConfig);
            GameData.RelicMainAffixData.TryGetValue(itemConfig.MainAffixGroup, out var mainAffixConfig);
            if (subAffixConfig == null || mainAffixConfig == null)
            {
                arg.SendMsg("物品无效");
                return;
            }

            int startIndex = 1;
            int mainAffixId;
            if (arg.BasicArgs[1].Contains(':'))
            {
                // 随机主词条
                mainAffixId = mainAffixConfig.Keys.ToList().RandomElement();
            }
            else
            {
                mainAffixId = int.Parse(arg.BasicArgs[1]);
                if (!mainAffixConfig.ContainsKey(mainAffixId))
                {
                    arg.SendMsg("主词条ID无效");
                    return;
                }
                startIndex++;
            }

            var remainLevel = 5;
            var subAffixes = new List<(int, int)>();
            for (var i = startIndex; i < arg.BasicArgs.Count; i++)
            {
                var subAffix = arg.BasicArgs[i].Split(':');
                if (subAffix.Length != 2 || !int.TryParse(subAffix[0], out var subId) || !int.TryParse(subAffix[1], out var subLevel))
                {
                    arg.SendMsg("参数无效");
                    return;
                }
                if (!subAffixConfig.ContainsKey(subId))
                {
                    arg.SendMsg("副词条ID无效");
                    return;
                }
                subAffixes.Add((subId, subLevel));
                remainLevel -= subLevel - 1;
            }
            if (subAffixes.Count < 4)
            {
                // 随机副词条
                var subAffixGroup = itemConfig.SubAffixGroup;
                var subAffixGroupConfig = GameData.RelicSubAffixData[subAffixGroup];
                var subAffixGroupKeys = subAffixGroupConfig.Keys.ToList();
                while (subAffixes.Count < 4)
                {
                    var subId = subAffixGroupKeys.RandomElement();
                    if (subAffixes.Any(x => x.Item1 == subId))
                    {
                        continue;
                    }
                    if (remainLevel <= 0)
                    {
                        subAffixes.Add((subId, 1));
                    }
                    else
                    {
                        var subLevel = Random.Shared.Next(1, Math.Min(remainLevel + 1, 5)) + 1;
                        subAffixes.Add((subId, subLevel));
                        remainLevel -= subLevel - 1;
                    }
                }
            }

            var itemData = new ItemData()
            {
                ItemId = int.Parse(arg.BasicArgs[0]),
                Level = Math.Max(Math.Min(level, 15), 1),
                UniqueId = ++player.InventoryManager!.Data.NextUniqueId,
                MainAffix = mainAffixId,
                Count = 1,
            };

            foreach (var (subId, subLevel) in subAffixes)
            {
                subAffixConfig.TryGetValue(subId, out var subAffix);
                var aff = new ItemSubAffix(subAffix!, 1);
                for (var i = 1; i < subLevel; i++)
                {
                    aff.IncreaseStep(subAffix!.StepNum);
                }
                itemData.SubAffixes.Add(aff);
            }

            for (var i = 0; i < amount; i++)
            {
                player.InventoryManager!.AddItem(itemData);
            }

            arg.SendMsg($"给予 @{player.Uid} {amount} 件遗器，所选主词条为 {mainAffixId}");
        }
    }
}
