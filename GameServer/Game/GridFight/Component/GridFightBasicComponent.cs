using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.GameServer.Game.GridFight.Sync;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.GridFight;
using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Proto.ServerSide;

namespace EggLink.DanhengServer.GameServer.Game.GridFight.Component;

public class GridFightBasicComponent(GridFightInstance inst) : BaseGridFightComponent(inst)
{
    public GridFightBasicInfoPb Data { get; set; } = new()
    {
        CurHp = 100,
        CurLevel = 1,
        CurOnGroundAvatarCount = 1,
        BuyLevelCost = 1,
        CurGold = 5
    };

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

    public async ValueTask<Retcode> BuyLevelExp(bool sendPacket = true)
    {
        if (!GameData.GridFightPlayerLevelData.TryGetValue(Data.CurLevel, out var levelConf) || levelConf.LevelUpExp == 0)
            return Retcode.RetGridFightGameplayLevelMax;

        // COST
        if (await UpdateGoldNum((int)-Data.BuyLevelCost, false) != Retcode.RetSucc)
            return Retcode.RetGridFightCoinNotEnough;

        Data.LevelExp += 1;

        // LEVEL UP
        if (Data.LevelExp >= levelConf.LevelUpExp)
        {
            await UpgradeLevel(1, false);
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

        Data.CurLevel += level;
        Data.LevelExp = 0;
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

    public uint GetFieldCount()
    {
        return 4 + 6;
    }

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
}