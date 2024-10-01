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
    }

    public List<RogueTournFormulaExcel> RogueFormulas { get; set; } = [];
    public Dictionary<int, RogueTournLevelInstance> Levels { get; set; } = [];
    public List<RogueTournDifficultyExcel> DifficultyExcels { get; set; } = [];
    public int CurLayerId { get; set; }
    public RogueTournAreaExcel AreaExcel { get; set; }
    public RogueTournLevelStatus LevelStatus { get; set; } = RogueTournLevelStatus.Processing;

    public Dictionary<RogueTournRoomTypeEnum, int> RoomTypeWeight { get; set; } = new()
    {
        { RogueTournRoomTypeEnum.Battle, 7 },
        { RogueTournRoomTypeEnum.Coin, 2 },
        { RogueTournRoomTypeEnum.Shop, 2 },
        { RogueTournRoomTypeEnum.Event, 3 },
        { RogueTournRoomTypeEnum.Adventure, 3 },
        { RogueTournRoomTypeEnum.Reward, 5 },
        { RogueTournRoomTypeEnum.Hidden, 1 }
    };

    public RogueTournLevelInstance? CurLevel => Levels.GetValueOrDefault(CurLayerId);

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

    public override async ValueTask RollBuff(int amount)
    {
        await RollBuff(amount, 2000101);
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
                var formulaList = GameData.RogueTournFormulaData.Values.Where(x => !RogueFormulas.Contains(x)).ToList();

                for (var i = 0; i < battle.Stages.Count; i++)
                {
                    await RollBuff(battle.Stages.Count, 2000103);

                    var menu = new RogueFormulaSelectMenu(this);
                    menu.RollFormula(formulaList);
                    var action = menu.GetActionInstance();
                    RogueActions.Add(action.QueuePosition, action);
                }

                await UpdateMenu();
            }
        }
        else
        {
            await RollBuff(battle.Stages.Count);
            await GainMoney(Random.Shared.Next(20, 60) * battle.Stages.Count);
        }
    }

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