using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.Data.Excel;
using EggLink.DanhengServer.Enums.Rogue;
using EggLink.DanhengServer.Enums.TournRogue;
using EggLink.DanhengServer.GameServer.Game.Battle;
using EggLink.DanhengServer.GameServer.Game.Player;
using EggLink.DanhengServer.GameServer.Game.Rogue;
using EggLink.DanhengServer.GameServer.Game.Rogue.Event;
using EggLink.DanhengServer.GameServer.Game.RogueTourn.Formula;
using EggLink.DanhengServer.GameServer.Game.RogueTourn.Scene;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.RogueCommon;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.RogueTourn;
using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Util;

namespace EggLink.DanhengServer.GameServer.Game.RogueTourn;

public class RogueTournInstance : BaseRogueInstance
{
    #region Initializer

    public RogueTournInstance(PlayerInstance player, int areaId) : base(player, RogueSubModeEnum.TournRogue, 0)
    {
        // generate levels
        foreach (var index in Enumerable.Range(1, 3))
        {
            var levelInstance = new RogueTournLevelInstance(index);
            Levels.Add(levelInstance.LayerId, levelInstance);
        }

        AreaExcel = GameData.RogueTournAreaData.GetValueOrDefault(areaId) ??
                    throw new Exception("Invalid area id"); // wont be null because of validation in RogueTournManager

        foreach (var difficulty in AreaExcel.DifficultyIDList)
            if (GameData.RogueTournDifficultyData.TryGetValue(difficulty, out var diff))
                DifficultyExcels.Add(diff);

        CurLayerId = 1101;
        EventManager = new RogueEventManager(player, this);

        var t = RollFormula(1, [RogueFormulaCategoryEnum.Common, RogueFormulaCategoryEnum.Rare]);
        t.AsTask().Wait();
    }

    #endregion

    #region Properties

    public List<RogueTournFormulaExcel> RogueFormulas { get; set; } = [];
    public List<int> ExpandedFormulaIdList { get; set; } = [];
    public Dictionary<int, RogueTournLevelInstance> Levels { get; set; } = [];
    public List<RogueTournDifficultyExcel> DifficultyExcels { get; set; } = [];
    public int CurLayerId { get; set; }
    public RogueTournAreaExcel AreaExcel { get; set; }
    public RogueTournLevelStatus LevelStatus { get; set; } = RogueTournLevelStatus.Processing;

    public Dictionary<RogueTournRoomTypeEnum, int> RoomTypeWeight { get; set; } = new()
    {
        { RogueTournRoomTypeEnum.Battle, 15 },
        { RogueTournRoomTypeEnum.Coin, 4 },
        { RogueTournRoomTypeEnum.Shop, 4 },
        { RogueTournRoomTypeEnum.Event, 7 },
        { RogueTournRoomTypeEnum.Adventure, 6 },
        { RogueTournRoomTypeEnum.Reward, 5 },
        { RogueTournRoomTypeEnum.Hidden, 1 }
    };

    public RogueTournLevelInstance? CurLevel => Levels.GetValueOrDefault(CurLayerId);

    #endregion

    #region Scene

    public async ValueTask EnterNextLayer(int roomIndex, RogueTournRoomTypeEnum type)
    {
        CurLayerId += 100;
        await EnterRoom(roomIndex, type);
    }

    public async ValueTask EnterRoom(int roomIndex, RogueTournRoomTypeEnum type)
    {
        if (CurLevel == null) return;

        //if (CurLevel.CurRoomIndex == roomIndex)
        //    // same room
        //    return;

        //if (CurLevel.CurRoomIndex + 1 != roomIndex) // only allow to enter next room
        //    // invalid room
        //    return;
        if (CurLevel.CurRoom != null)
            CurLevel.CurRoom.Status = RogueTournRoomStatus.Finish;

        // enter room
        CurLevel.CurRoomIndex = roomIndex;
        CurLevel.CurRoom?.Init(type);

        // scene
        var entrance = CurLevel.CurRoom?.Config?.EntranceId ?? 0;
        var group = CurLevel.CurRoom?.Config?.AnchorGroup ?? 0;
        var anchor = CurLevel.CurRoom?.Config?.AnchorId ?? 1;

        // call event
        EventManager?.OnNextRoom();
        foreach (var miracle in RogueMiracles.Values) miracle.OnEnterNextRoom();

        await Player.EnterMissionScene(entrance, group, anchor, false);

        // sync
        await Player.SendPacket(new PacketRogueTournLevelInfoUpdateScNotify(this, [CurLevel]));
    }

    public async ValueTask QuitRogue()
    {
        await Player.EnterMissionScene(1034102, 0, 0, false);
        Player.RogueTournManager!.RogueTournInstance = null;
    }

    #endregion

    #region Buff & Formula

    public override async ValueTask RollBuff(int amount)
    {
        await RollBuff(amount, 2000101);
    }

    public async ValueTask RollFormula(int amount, List<RogueFormulaCategoryEnum> categories)
    {
        var formulaList = GameData.RogueTournFormulaData.Values.Where(x => !RogueFormulas.Contains(x) && categories.Contains(x.FormulaCategory)).ToList();

        for (var i = 0; i < amount; i++)
        {
            var menu = new RogueFormulaSelectMenu(this);
            menu.RollFormula(formulaList);
            var action = menu.GetActionInstance();
            RogueActions.Add(action.QueuePosition, action);
        }

        await UpdateMenu();
    }

    public override async ValueTask HandleBuffSelect(int buffId)
    {
        await base.HandleBuffSelect(buffId);
        await ExpandFormula();
    }

    public override async ValueTask<RogueCommonActionResult?> AddBuff(int buffId, int level = 1,
        RogueCommonActionResultSourceType source = RogueCommonActionResultSourceType.Dialogue,
        RogueCommonActionResultDisplayType displayType = RogueCommonActionResultDisplayType.Single,
        bool updateMenu = true, bool notify = true)
    {
        var res = await base.AddBuff(buffId, level, source, displayType, updateMenu, notify);

        await ExpandFormula();

        return res;
    }

    public async ValueTask ExpandFormula()
    {
        // expand formula
        foreach (var formula in RogueFormulas)
        {
            if (formula.IsExpanded(RogueBuffs.Select(x => x.BuffId).ToList()) &&
                !ExpandedFormulaIdList.Contains(formula.FormulaID))
            {
                ExpandedFormulaIdList.Add(formula.FormulaID);
                await Player.SendPacket(new PacketSyncRogueCommonActionResultScNotify(RogueSubMode,
                    formula.ToResultProto(RogueCommonActionResultSourceType.Select,
                        RogueBuffs.Select(x => x.BuffId).ToList())));
            }

            else if (!formula.IsExpanded(RogueBuffs.Select(x => x.BuffId).ToList()) &&
                     ExpandedFormulaIdList.Contains(formula.FormulaID))  // remove expanded formula
            {
                ExpandedFormulaIdList.Remove(formula.FormulaID);
                await Player.SendPacket(new PacketSyncRogueCommonActionResultScNotify(RogueSubMode,
                    formula.ToResultProto(RogueCommonActionResultSourceType.Select,
                        RogueBuffs.Select(x => x.BuffId).ToList())));
            }
        }
    }

    public async ValueTask HandleFormulaSelect(int formulaId)
    {
        if (RogueActions.Count == 0) return;

        var action = RogueActions.First().Value;
        if (action.RogueFormulaSelectMenu != null)
        {
            var formula = action.RogueFormulaSelectMenu.Formulas.Find(x => x.FormulaID == formulaId);
            if (formula != null) // check if buff is in the list
                if (!RogueFormulas.Exists(x => x.FormulaID == formulaId)) // check if buff already exists
                {
                    RogueFormulas.Add(formula);
                    await Player.SendPacket(new PacketSyncRogueCommonActionResultScNotify(RogueSubMode,
                        formula.ToResultProto(RogueCommonActionResultSourceType.Select,
                            RogueBuffs.Select(x => x.BuffId).ToList())));
                }

            RogueActions.Remove(action.QueuePosition);
        }

        await UpdateMenu();

        await Player.SendPacket(
            new PacketHandleRogueCommonPendingActionScRsp(action.QueuePosition, selectFormula: true));
    }

    #endregion

    #region Handlers

    public override void OnBattleStart(BattleInstance battle)
    {
        base.OnBattleStart(battle);

        if (DifficultyExcels.Count > 0)
        {
            var diff = DifficultyExcels.RandomElement();
            if (diff.LevelList.Count > 0)
                battle.CustomLevel = diff.LevelList.RandomElement();
        }

        foreach (var formula in RogueFormulas.Where(formula =>
                     formula.IsExpanded(RogueBuffs.Select(x => x.BuffId).ToList()) &&
                     formula.FormulaCategory != RogueFormulaCategoryEnum.PathEcho))
            // apply formula effect
            battle.Buffs.Add(new MazeBuff(formula.MazeBuffID, 1, -1)
            {
                WaveFlag = -1
            });
    }

    public override async ValueTask OnBattleEnd(BattleInstance battle, PVEBattleResultCsReq req)
    {
        foreach (var miracle in RogueMiracles.Values) miracle.OnEndBattle(battle);

        if (req.EndStatus != BattleEndStatus.BattleEndWin)
            // quit
            //await QuitRogue();
            return;

        if (CurLevel?.Rooms.Last().RoomIndex == CurLevel?.CurRoom?.RoomIndex)
        {
            // layer last room
            if (Levels.Keys.Last() == CurLayerId)
            {
                // last layer
            }
            else
            {
                // trigger formula
                await RollBuff(battle.Stages.Count, 2000103);
                await RollFormula(battle.Stages.Count, [RogueFormulaCategoryEnum.Legendary]);
            }
        }
        else
        {
            await RollBuff(battle.Stages.Count);
            await GainMoney(Random.Shared.Next(20, 60) * battle.Stages.Count);
        }
    }

    #endregion

    #region Workbench

    public async ValueTask<Retcode> HandleFunc(RogueWorkbenchFunc func, RogueWorkbenchContentInfo content)
    {
        switch (func.Excel.FuncType)
        {
            case RogueTournWorkbenchFuncTypeEnum.BuffEnhance:
                return await HandleBuffEnhance(func, content);
            case RogueTournWorkbenchFuncTypeEnum.BuffReforge:
                return await HandleBuffReforge(func, content);
        }

        return Retcode.RetSucc;
    }

    public async ValueTask<Retcode> HandleBuffEnhance(RogueWorkbenchFunc func, RogueWorkbenchContentInfo content)
    {
        var buffId = content.EnhanceBuffFunc.TargetBuffId;
        var buff = RogueBuffs.Find(x => x.BuffId == buffId);
        if (buff == null) return Retcode.RetRogueSelectBuffNotExist;

        if (buff.BuffLevel == 2) return Retcode.RetRogueSelectBuffCertainMismatch;  // already enhanced

        var cost = (int)buff.BuffExcel.RogueBuffCategory;
        if (func.CurNum < cost) return Retcode.RetRogueCoinNotEnough;

        func.CurNum -= cost;

        await EnhanceBuff(buff.BuffId, RogueCommonActionResultSourceType.Enhance);

        return Retcode.RetSucc;
    }

    public async ValueTask<Retcode> HandleBuffReforge(RogueWorkbenchFunc func, RogueWorkbenchContentInfo content)
    {
        var buffId = content.ReforgeBuffFunc.HMJAFJLBPAM;
        var buff = RogueBuffs.Find(x => x.BuffId == buffId);
        if (buff == null) return Retcode.RetRogueSelectBuffNotExist;

        var cost = func.CurCost;
        if (CurMoney < cost) return Retcode.RetRogueCoinNotEnough;
        func.CurCost += 30;

        // TODO: remove old buff


        //await ReforgeBuff(buff.BuffId, RogueCommonActionResultSourceType.Reforge);

        return Retcode.RetSucc;
    }

    #endregion

    #region Serilization

    public RogueTournCurInfo ToProto()
    {
        return new RogueTournCurInfo
        {
            RogueTournCurGameInfo = ToCurGameInfo(),
            RogueTournCurAreaInfo = ToCurAreaInfo()
        };
    }

    public RogueTournCurGameInfo ToCurGameInfo()
    {
        return new RogueTournCurGameInfo
        {
            Buff = ToBuffInfo(),
            ItemValue = ToGameItemValueInfo(),
            Level = ToLevelInfo(),
            Lineup = ToLineupInfo(),
            MiracleInfo = ToMiracleInfo(),
            RogueTournGameAreaInfo = ToGameAreaInfo(),
            TournFormulaInfo = ToFormulaInfo()
        };
    }

    public ChessRogueBuffInfo ToBuffInfo()
    {
        return new ChessRogueBuffInfo
        {
            ChessRogueBuffInfo_ = new ChessRogueBuff
            {
                BuffList = { RogueBuffs.Select(x => x.ToCommonProto()) }
            }
        };
    }

    public RogueGameItemValue ToGameItemValueInfo()
    {
        return new RogueGameItemValue
        {
            VirtualItem = { { 31, (uint)CurMoney } }
        };
    }

    public RogueTournLineupInfo ToLineupInfo()
    {
        return new RogueTournLineupInfo
        {
            AvatarIdList = { Player.LineupManager!.GetCurLineup()!.BaseAvatars!.Select(x => (uint)x.BaseAvatarId) },
            RogueReviveCost = new ItemCostData
            {
                ItemList =
                {
                    new ItemCost
                    {
                        PileItem = new PileItem
                        {
                            ItemId = 31,
                            ItemNum = (uint)CurReviveCost
                        }
                    }
                }
            }
        };
    }

    public ChessRogueMiracleInfo ToMiracleInfo()
    {
        var proto = new ChessRogueMiracleInfo
        {
            ChessRogueMiracleInfo_ = new ChessRogueMiracle()
        };

        proto.ChessRogueMiracleInfo_.MiracleList.AddRange(RogueMiracles.Select(x => x.Value.ToGameMiracleProto())
            .ToList());

        return proto;
    }


    public RogueTournLevelInfo ToLevelInfo()
    {
        var proto = new RogueTournLevelInfo
        {
            Status = LevelStatus,
            CurLevelIndex = (uint)(CurLevel?.CurRoomIndex ?? 0),
            Reason = RogueTournSettleReason.None
        };

        foreach (var levelInstance in Levels.Values) proto.LevelInfoList.Add(levelInstance.ToProto());

        return proto;
    }

    public RogueTournFormulaInfo ToFormulaInfo()
    {
        var proto = new RogueTournFormulaInfo
        {
            FormulaTypeValue = new FormulaTypeValue()
        };

        foreach (var formula in RogueFormulas)
            proto.GameFormulaInfo.Add(formula.ToProto(RogueBuffs.Select(x => x.BuffId).ToList()));

        return proto;
    }

    public RogueTournGameAreaInfo ToGameAreaInfo()
    {
        var proto = new RogueTournGameAreaInfo
        {
            GameAreaId = (uint)AreaExcel.AreaID
        };

        return proto;
    }

    public RogueTournCurAreaInfo ToCurAreaInfo()
    {
        var proto = new RogueTournCurAreaInfo
        {
            RogueSubMode = (uint)RogueSubMode,
            SubAreaId = (uint)AreaExcel.AreaID,
            PendingAction = RogueActions.Count > 0
                ? RogueActions.First().Value.ToProto()
                : new RogueCommonPendingAction() // to serialize empty action
        };

        return proto;
    }

    public RogueTournCurSceneInfo ToCurSceneInfo()
    {
        return new RogueTournCurSceneInfo
        {
            Lineup = Player.LineupManager!.GetCurLineup()!.ToProto(),
            Scene = Player.SceneInstance!.ToProto(),
            //RotateInfo = new RogueMapRotateInfo
            //{
            //    IsRotate = CurLevel?.CurRoom?.Config?.RotateInfo.IsRotate ?? false,
            //    BJPBAJECKFO = (uint)(CurLevel?.CurRoom?.Config?.RotateInfo.RotateNum ?? 0)  // HDEHHKEMOCD
            //}
            RotateInfo = new RogueMapRotateInfo()
        };
    }

    #endregion
}