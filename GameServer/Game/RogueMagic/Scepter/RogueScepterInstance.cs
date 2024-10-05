using EggLink.DanhengServer.Data.Excel;
using EggLink.DanhengServer.GameServer.Game.RogueMagic.MagicUnit;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Game.RogueMagic.Scepter;

public class RogueScepterInstance(RogueMagicScepterExcel excel)
{
    public RogueMagicScepterExcel Excel { get; set; } = excel;
    public Dictionary<int, RogueMagicUnitInstance> DressedUnits { get; set; } = [];

    public void AddUnit(int slot, RogueMagicUnitInstance unit)
    {
        DressedUnits[slot] = unit;
    }

    public void RemoveUnit(int slot)
    {
        DressedUnits.Remove(slot);
    }

    public RogueMagicGameScepterInfo ToProto()
    {
        var proto = new RogueMagicGameScepterInfo
        {
            ModifierContent = ToBasicInfo(),
            ScepterDressInfo = { DressedUnits.Select(x => new RogueMagicScepterDressInfo
            {
                Slot = (uint)x.Key,
                DressMagicUnitUniqueId = (uint)x.Value.UniqueId,
                Type = (uint)x.Value.Excel.MagicUnitType
            }) }
        };

        foreach (var trench in Excel.TrenchCount)
        {
            proto.TrenchCount.Add((uint)trench.Key, (uint)trench.Value);
        }

        foreach (var unitInfo in Excel.LockMagicUnit)
        {
            proto.LockedMagicUnitList.Add(new RogueMagicGameUnit
            {
                MagicUnitId = (uint)unitInfo.MagicUnitId,
                Level = (uint)unitInfo.MagicUnitLevel
            });
        }

        return proto;
    }

    public RogueCommonActionResult ToGetInfo(RogueCommonActionResultSourceType source)
    {
        return new RogueCommonActionResult
        {
            Source = source,
            RogueAction = new RogueCommonActionResultData
            {
                GetScepterList = ToGetInfo()
            }
        };
    }

    public RogueCommonActionResult ToDressInfo(RogueCommonActionResultSourceType source)
    {
        return new RogueCommonActionResult
        {
            Source = source,
            RogueAction = new RogueCommonActionResultData
            {
                DressScepterList = ToDressInfo()
            }
        };
    }

    public BattleRogueMagicScepter ToBattleScepterInfo()
    {
        var proto = new BattleRogueMagicScepter
        {
            ScepterId = (uint)Excel.ScepterID,
            Level = (uint)Excel.ScepterLevel
        };

        foreach (var trench in Excel.TrenchCount)
        {
            proto.TrenchCount.Add((uint)trench.Key, (uint)trench.Value);
        }

        foreach (var unitInfo in Excel.LockMagicUnit)
        {
            proto.RogueMagicUnitInfoList.Add(new BattleRogueMagicUnit
            {
                MagicUnitId = (uint)unitInfo.MagicUnitId,
                Level = (uint)unitInfo.MagicUnitLevel
            });
        }

        foreach (var unitInfo in DressedUnits)
        {
            proto.RogueMagicUnitInfoList.Add(new BattleRogueMagicUnit
            {
                MagicUnitId = (uint)unitInfo.Value.Excel.MagicUnitID,
                Level = (uint)unitInfo.Value.Excel.MagicUnitLevel,
                DiceSlotId = (uint)unitInfo.Key
            });
        }

        return proto;
    }

    public RogueMagicScepter ToBasicInfo()
    {
        var proto = new RogueMagicScepter
        {
            ScepterId = (uint)Excel.ScepterID,
            Level = (uint)Excel.ScepterLevel
        };

        return proto;
    }

    public RogueCommonGetScepter ToGetInfo()
    {
        var proto = new RogueCommonGetScepter
        {
            UpdateScepterInfo = ToProto()
        };

        return proto;
    }

    public RogueCommonDressScepter ToDressInfo()
    {
        var proto = new RogueCommonDressScepter
        {
            UpdateScepterInfo = ToProto()
        };

        return proto;
    }
}