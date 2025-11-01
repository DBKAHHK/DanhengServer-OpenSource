using EggLink.DanhengServer.GameServer.Game.Battle;
using EggLink.DanhengServer.GameServer.Game.GridFight.Component;
using EggLink.DanhengServer.GameServer.Game.GridFight.PendingAction;
using EggLink.DanhengServer.GameServer.Game.GridFight.Sync;
using EggLink.DanhengServer.GameServer.Game.Player;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.GridFight;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Game.GridFight;

public class GridFightInstance(PlayerInstance player, uint season, uint divisionId, bool isOverLock, uint uniqueId)
{
    public uint Season { get; } = season;
    public uint DivisionId { get; } = divisionId;
    public bool IsOverLock { get; } = isOverLock;
    public uint UniqueId { get; } = uniqueId;
    public List<BaseGridFightComponent> Components { get; } = [];
    public PlayerInstance Player { get; } = player;

    public BattleInstance? StartBattle()
    {
        var battle = Player.BattleManager!.StartGridFightBattle(this);

        return battle;
    }

    public async ValueTask EndBattle(BattleInstance battle, PVEBattleResultCsReq req)
    {
        if (battle.BattleEndStatus == BattleEndStatus.BattleEndQuit) return;

        List<BaseGridFightSyncData> syncs = [];

        var basicComp = GetComponent<GridFightBasicComponent>();
        var levelComp = GetComponent<GridFightLevelComponent>();
        var prevData = basicComp.Data.Clone();

        var expNum = 5u;
        var baseCoin = levelComp.CurrentSection.Excel.BasicGoldRewardNum;
        var interestCoin = basicComp.Data.CurGold / 10;

        if (battle.BattleEndStatus == BattleEndStatus.BattleEndWin)
        {
            basicComp.Data.ComboNum++;
        }
        else
        {
            basicComp.Data.ComboNum = 0;

            // cost hp
            await basicComp.UpdateLineupHp(-5, false);
        }

        var comboCoin = basicComp.Data.ComboNum switch
        {
            >= 5 => 3u,
            2 or 3 or 4 => 2u,
            0 => 0u,
            _ => 1u
        };

        await basicComp.UpdateGoldNum((int)(baseCoin + interestCoin + comboCoin), false, GridFightSrc.KGridFightSrcNone);
        await basicComp.AddLevelExp(expNum, false);

        List<GridFightRoleDamageSttInfo> sttList = [];
        foreach (var roleBattleStt in req.Stt.GridFightBattleStt.RoleBattleStt)
        {
            var res = await levelComp.AddRoleDamageStt(roleBattleStt.RoleBasicId, roleBattleStt.Damage, false);
            if (res.Item2 != null)
                sttList.Add(res.Item2);
        }

        var curData = basicComp.Data.Clone();
        await Player.SendPacket(new PacketGridFightEndBattleStageNotify(this, expNum, prevData, curData,
            sttList, battle.BattleEndStatus == BattleEndStatus.BattleEndWin, baseCoin, interestCoin,
            comboCoin));


        syncs.Add(new GridFightGoldSyncData(GridFightSrc.KGridFightSrcBattleEnd, basicComp.Data));
        syncs.Add(new GridFightPlayerLevelSyncData(GridFightSrc.KGridFightSrcBattleEnd, basicComp.Data));
        syncs.Add(new GridFightLineupHpSyncData(GridFightSrc.KGridFightSrcBattleEnd, basicComp.Data));
        syncs.Add(new GridFightComboNumSyncData(GridFightSrc.KGridFightSrcBattleEnd, basicComp.Data));
        syncs.Add(new GridFightRoleDamageSttSyncData(GridFightSrc.KGridFightSrcBattleEnd, levelComp));
        syncs.AddRange(await levelComp.EnterNextSection(false));

        await Player.SendPacket(new PacketGridFightSyncUpdateResultScNotify(syncs));
    }

    public void InitializeComponents()
    {
        Components.Add(new GridFightBasicComponent(this));
        Components.Add(new GridFightShopComponent(this));
        Components.Add(new GridFightLevelComponent(this));
        Components.Add(new GridFightRoleComponent(this));

        _ = GetComponent<GridFightRoleComponent>().AddAvatar(1414, 3, false);
        _ = GetComponent<GridFightShopComponent>().RefreshShop(true, false);

        _ = CreatePendingAction<GridFightPortalBuffPendingAction>();  // TODO wait for release official server
    }

    public T GetComponent<T>() where T : BaseGridFightComponent
    {
        return (T)Components.First(c => c is T);
    }

    public GridFightCurrentInfo ToProto()
    {
        return new GridFightCurrentInfo
        {
            DivisionId = DivisionId,
            Season = Season,
            IsOverlock = IsOverLock,
            UniqueId = UniqueId,
            PendingAction = GetCurAction().ToProto(),
            GridFightGameData = ToGameDataInfo(),
            RogueCurrentGameInfo = { ToGameInfos() }
        };
    }

    public List<GridFightGameInfo> ToGameInfos()
    {
        return (from c in Components select c.ToProto()).ToList();
    }

    public GridFightGameData ToGameDataInfo()
    {
        return new GridFightGameData();
    }

    #region Pending Action

    public SortedDictionary<uint, BaseGridFightPendingAction> PendingActions { get; set; } = new();
    private uint _curQueuePos = 1;

    public BaseGridFightPendingAction GetCurAction()
    {
        if (PendingActions.Count > 0)
        {
            return PendingActions.First().Value;
        }

        return new GridFightEmptyPendingAction(this);
    }

    public uint AddPendingAction(BaseGridFightPendingAction action)
    {
        var pos = _curQueuePos++;

        action.QueuePosition = pos;
        PendingActions[pos] = action;

        return pos;
    }

    public async ValueTask<BaseGridFightSyncData> CreatePendingAction<T>(GridFightSrc src = GridFightSrc.KGridFightSrcEnterNode, bool sendPacket = true) where T: BaseGridFightPendingAction
    {
        var action = (T)Activator.CreateInstance(typeof(T), this)!;

        AddPendingAction(action);

        var res = new GridFightPendingActionSyncData(src, action);
        if (sendPacket)
           await Player.SendPacket(new PacketGridFightSyncUpdateResultScNotify(res));

        return res;
    }

    public async ValueTask HandleResultRequest(GridFightHandlePendingActionCsReq req)
    {
        var curAction = GetCurAction();

        // end
        PendingActions.Remove(curAction.QueuePosition);
        var src = GridFightSrc.KGridFightSrcNone;

        switch (req.GridFightActionTypeCase)
        {
            case GridFightHandlePendingActionCsReq.GridFightActionTypeOneofCase.PortalBuffAction:
                src = GridFightSrc.KGridFightSrcSelectPortalBuff;
                break;
            case GridFightHandlePendingActionCsReq.GridFightActionTypeOneofCase.PortalBuffRerollAction:
                if (curAction is GridFightPortalBuffPendingAction portalBuffAction)
                {
                    await portalBuffAction.RerollBuff();
                }

                break;
            case GridFightHandlePendingActionCsReq.GridFightActionTypeOneofCase.AugmentAction:
                src = GridFightSrc.KGridFightSrcSelectAugment;
                await CheckCurNodeFinish(src);
                break;

            case GridFightHandlePendingActionCsReq.GridFightActionTypeOneofCase.RerollAugmentAction:
                if (curAction is GridFightAugmentPendingAction augmentAction)
                {
                    await augmentAction.RerollAugment(req.RerollAugmentAction.AugmentId);
                }

                break;
            case GridFightHandlePendingActionCsReq.GridFightActionTypeOneofCase.EliteAction:
                break;
            case GridFightHandlePendingActionCsReq.GridFightActionTypeOneofCase.SupplyAction:
                src = GridFightSrc.KGridFightSrcSelectSupply;
                await CheckCurNodeFinish(src);
                break;
        }


        await Player.SendPacket(new PacketGridFightSyncUpdateResultScNotify(
            new GridFightPendingActionSyncData(src, GetCurAction())));
    }

    public async ValueTask CheckCurNodeFinish(GridFightSrc src)
    {
        var levelComp = GetComponent<GridFightLevelComponent>();
        var curSection = levelComp.CurrentSection;

        if (curSection.Encounters.Count != 0) return;

        if (PendingActions.Count != 0) return;

        // next
        await levelComp.EnterNextSection(src:src);
    }

    #endregion

    #region Portal Buff

    public async ValueTask AddPortalBuff(uint portalBuffId)
    {

    }

    #endregion
}