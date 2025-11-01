using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.GameServer.Game.GridFight.PendingAction;
using EggLink.DanhengServer.GameServer.Game.GridFight.Sync;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.GridFight;
using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Proto.ServerSide;

namespace EggLink.DanhengServer.GameServer.Game.GridFight.Component;

public class GridFightBasicComponent(GridFightInstance inst) : BaseGridFightComponent(inst)
{
    #region Fields & Properties

    public const uint MaxHp = 100;

    public GridFightBasicInfoPb Data { get; set; } = new()
    {
        CurHp = 100,
        CurLevel = 1,
        CurOnGroundAvatarCount = 1,
        BuyLevelCost = 1,
        CurGold = 5
    };

    #endregion

    #region Data Management

    public async ValueTask<Retcode> UpdateGoldNum(int changeNum, bool sendPacket = true, GridFightSrc src = GridFightSrc.KGridFightSrcManualRefreshGoods)
    {
        if (changeNum < 0 && -changeNum > Data.CurGold)
        {
            return Retcode.RetGridFightCoinNotEnough;
        }

        Data.CurGold = (uint)(Data.CurGold + changeNum);

        if (sendPacket)
        {
            await Inst.Player.SendPacket(new PacketGridFightSyncUpdateResultScNotify(new GridFightGoldSyncData(src, Data)));
        }

        return Retcode.RetSucc;
    }

    public async ValueTask<Retcode> UpdateLineupHp(int changeNum, bool sendPacket = true, GridFightSrc src = GridFightSrc.KGridFightSrcBattleEnd)
    {
        Data.CurHp = (uint)Math.Min(Math.Max(Data.CurHp + changeNum, 0), MaxHp);

        if (sendPacket)
        {
            await Inst.Player.SendPacket(new PacketGridFightSyncUpdateResultScNotify(new GridFightLineupHpSyncData(src, Data)));
        }

        return Retcode.RetSucc;
    }

    public async ValueTask<Retcode> BuyLevelExp(bool sendPacket = true)
    {
        if (!GameData.GridFightPlayerLevelData.TryGetValue(Data.CurLevel, out var levelConf) || levelConf.LevelUpExp == 0)
            return Retcode.RetGridFightGameplayLevelMax;

        // COST
        if (await UpdateGoldNum((int)-Data.BuyLevelCost, false) != Retcode.RetSucc)
            return Retcode.RetGridFightCoinNotEnough;

        return await AddLevelExp(1, sendPacket);
    }

    public async ValueTask<Retcode> AddLevelExp(uint exp, bool sendPacket = true)
    {
        var upperLevels = GameData.GridFightPlayerLevelData.Values.Where(x => x.PlayerLevel >= Data.CurLevel)
            .OrderBy(x => x.PlayerLevel).ToList();

        if (upperLevels.Count == 1)  // 1 contain cur level
            return Retcode.RetGridFightGameplayLevelMax;  // already max level

        Data.LevelExp += exp;

        // LEVEL UP
        var costExp = 0;
        var targetLevel = Data.CurLevel;

        foreach (var level in upperLevels)
        {
            if (level.LevelUpExp + costExp > Data.LevelExp)
                break;

            costExp += (int)level.LevelUpExp;
            targetLevel = level.PlayerLevel + 1;
        }

        if (targetLevel > Data.CurLevel)
        {
            await UpgradeLevel(targetLevel - Data.CurLevel, false);
        }

        if (sendPacket)
        {
            await Inst.Player.SendPacket(new PacketGridFightSyncUpdateResultScNotify(
                new GridFightGoldSyncData(GridFightSrc.KGridFightSrcBuyGoods, Data),
                new GridFightPlayerLevelSyncData(GridFightSrc.KGridFightSrcBuyGoods, Data),
                new GridFightMaxAvatarNumSyncData(GridFightSrc.KGridFightSrcBuyGoods, Data),
                new GridFightBuyExpCostSyncData(GridFightSrc.KGridFightSrcBuyGoods, Data)));
        }

        return Retcode.RetSucc;
    }

    public async ValueTask<Retcode> UpgradeLevel(uint level, bool sendPacket = true)
    {
        if (!GameData.GridFightPlayerLevelData.TryGetValue(level + Data.CurLevel, out var levelConf))
            return Retcode.RetGridFightGameplayLevelMax;

        // adjust exp and other stats
        for (var i = Data.CurLevel; i < level + Data.CurLevel; i++)
        {
            if (!GameData.GridFightPlayerLevelData.TryGetValue(i, out var curLevelConf))
                break;

            Data.LevelExp -= curLevelConf.LevelUpExp;
        }

        Data.CurLevel += level;

        Data.BuyLevelCost = (uint)Math.Ceiling(Data.CurLevel / 2f);
        Data.CurOnGroundAvatarCount = levelConf.AvatarMaxNumber;

        if (sendPacket)
        {
            await Inst.Player.SendPacket(new PacketGridFightSyncUpdateResultScNotify(
                new GridFightGoldSyncData(GridFightSrc.KGridFightSrcBuyGoods, Data),
                new GridFightPlayerLevelSyncData(GridFightSrc.KGridFightSrcBuyGoods, Data),
                new GridFightMaxAvatarNumSyncData(GridFightSrc.KGridFightSrcBuyGoods, Data),
                new GridFightBuyExpCostSyncData(GridFightSrc.KGridFightSrcBuyGoods, Data)));
        }

        return Retcode.RetSucc;
    }

    #endregion

    #region Information

    public uint GetFieldCount()
    {
        return 4 + 6;
    }

    #endregion

    #region Serialization

    public override GridFightGameInfo ToProto()
    {
        return new GridFightGameInfo
        {
            GridBasicInfo = new GridFightGameBasicInfo
            {
                GridFightCurLevel = Data.CurLevel,
                GridFightCurLevelExp = Data.LevelExp,
                GridFightLevelCost = Data.BuyLevelCost,
                GridFightMaxAvatarCount = Data.CurOnGroundAvatarCount,
                GridFightOffFieldMaxCount = 6,
                GridFightMaxFieldCount = 8,
                GridFightLineupHp = Data.CurHp,
                GridFightCurGold = Data.CurGold,
                GridFightMaxGold = 2000,
                GridFightComboWinNum = Data.ComboNum,
                OCMGMEHECBB = new OPIBBPCHFII
                {
                    IJDIAOMINLB = new BHJALAPDBLH()
                },
                CALCJMHAKPF = new OLEIDBLBILD
                {
                }
            }
        };
    }

    #endregion
}