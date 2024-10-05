using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.Data.Excel;
using EggLink.DanhengServer.Enums.Rogue;
using EggLink.DanhengServer.Enums.RogueMagic;
using EggLink.DanhengServer.GameServer.Game.Battle;
using EggLink.DanhengServer.GameServer.Game.Player;
using EggLink.DanhengServer.GameServer.Game.Rogue;
using EggLink.DanhengServer.GameServer.Game.Rogue.Event;
using EggLink.DanhengServer.GameServer.Game.RogueMagic.MagicUnit;
using EggLink.DanhengServer.GameServer.Game.RogueMagic.Scene;
using EggLink.DanhengServer.GameServer.Game.RogueMagic.Scepter;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.RogueCommon;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.RogueMagic;
using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Util;

namespace EggLink.DanhengServer.GameServer.Game.RogueMagic;

public class RogueMagicInstance : BaseRogueInstance
{
    #region Initializer

    public RogueMagicInstance(PlayerInstance player, int areaId, List<int> difficultyIds, int styleType) : base(player, RogueSubModeEnum.MagicRogue, 0)
    {
        // generate levels
        AreaExcel = GameData.RogueMagicAreaData.GetValueOrDefault(areaId) ??
                    throw new Exception("Invalid area id"); // wont be null because of validation in RogueTournManager

        foreach (var index in Enumerable.Range(1, AreaExcel.LayerIDList.Count))
        {
            var levelInstance = new RogueMagicLevelInstance(index, AreaExcel.LayerIDList[index - 1]);
            Levels.Add(levelInstance.LayerId, levelInstance);
        }

        CurLayerId = AreaExcel.LayerIDList[0];
        EventManager = new RogueEventManager(player, this);

        BaseRerollCount = 0;

        foreach (var id in difficultyIds)
        {
            GameData.RogueMagicDifficultyCompData.TryGetValue(id, out var excel);
            if (excel != null)
                DifficultyCompExcels.Add(excel);
        }

        StyleType = (RogueMagicStyleTypeEnum)styleType;

        var t = RollScepter(1, 1);
        t.AsTask().Wait();
    }

    #endregion

    #region Properties

    public RogueMagicAreaExcel AreaExcel { get; set; }
    public List<RogueMagicDifficultyCompExcel> DifficultyCompExcels { get; set; } = [];
    public Dictionary<int, RogueMagicLevelInstance> Levels { get; set; } = [];

    public int CurLayerId { get; set; }
    public RogueMagicLevelInstance? CurLevel => Levels.GetValueOrDefault(CurLayerId);

    public RogueMagicLevelStatus LevelStatus { get; set; } = RogueMagicLevelStatus.Processing;
    public RogueMagicStyleTypeEnum StyleType { get; set; }

    public Dictionary<int, RogueScepterInstance> RogueScepters { get; set; } = [];
    public Dictionary<int, RogueMagicUnitInstance> RogueMagicUnits { get; set; } = [];
    public int CurMagicUnitUniqueId { get; set; } = 1;

    public Dictionary<RogueMagicRoomTypeEnum, int> RoomTypeWeight { get; set; } = new()
    {
        { RogueMagicRoomTypeEnum.Battle, 15 },
        { RogueMagicRoomTypeEnum.Wealth, 4 },
        { RogueMagicRoomTypeEnum.Shop, 4 },
        { RogueMagicRoomTypeEnum.Event, 7 },
        { RogueMagicRoomTypeEnum.Adventure, 6 },
        { RogueMagicRoomTypeEnum.Reward, 5 },
        { RogueMagicRoomTypeEnum.Elite, 1 }
    };

    #endregion

    #region Scene

    public async ValueTask EnterNextLayer(int roomIndex, RogueMagicRoomTypeEnum type)
    {
        var curIndex = CurLevel?.LevelIndex ?? 0;
        if (curIndex == 3)
        {
            // last layer
            return;
        }

        CurLayerId = AreaExcel.LayerIDList[curIndex - 1];
        await EnterRoom(roomIndex, type);
    }

    public async ValueTask EnterRoom(int roomIndex, RogueMagicRoomTypeEnum type)
    {
        if (CurLevel == null) return;

        //if (CurLevel.CurRoomIndex == roomIndex)
        //    // same room
        //    return;

        //if (CurLevel.CurRoomIndex + 1 != roomIndex) // only allow to enter next room
        //    // invalid room
        //    return;
        if (CurLevel.CurRoom != null)
            CurLevel.CurRoom.Status = RogueMagicRoomStatus.Finish;

        // enter room
        CurLevel.CurRoomIndex = roomIndex;
        CurLevel.CurRoom?.Init(type);

        // next room
        CurActionQueuePosition += 15;
        var next = CurLevel.Rooms.Find(x => x.RoomIndex == roomIndex + 1);
        if (next != null)
            next.Status = RogueMagicRoomStatus.Inited;

        // scene
        var entrance = CurLevel.CurRoom?.Config?.EntranceId ?? 0;
        var group = CurLevel.CurRoom?.Config?.AnchorGroup ?? 0;
        var anchor = CurLevel.CurRoom?.Config?.AnchorId ?? 1;

        // call event
        EventManager?.OnNextRoom();
        foreach (var miracle in RogueMiracles.Values) miracle.OnEnterNextRoom();

        await Player.EnterMissionScene(entrance, group, anchor, false);

        // sync
        await Player.SendPacket(new PacketRogueMagicLevelInfoUpdateScNotify(this, [CurLevel]));
    }

    public async ValueTask QuitRogue()
    {
        await Player.EnterMissionScene(801120102, 0, 0, false);
        Player.RogueMagicManager!.RogueMagicInstance = null;
    }

    #endregion

    #region Scepter

    public async ValueTask RollScepter(int amount, int level)
    {
        var scepterExcels = GameData.RogueMagicScepterData.Values.Where(x => !RogueScepters.ContainsKey(x.ScepterID) && x.ScepterLevel == level).ToList();

        for (var i = 0; i < amount; i++)
        {
            var menu = new RogueScepterSelectMenu(this);
            menu.RollScepter(scepterExcels);
            var action = menu.GetActionInstance();
            RogueActions.Add(action.QueuePosition, action);
        }

        await UpdateMenu();
    }

    public async ValueTask HandleScepterSelect(RogueMagicScepter selectScepter)
    {
        if (RogueActions.Count == 0) return;

        var action = RogueActions.First().Value;
        if (action.RogueScepterSelectMenu != null)
        {
            var scepterExcel = action.RogueScepterSelectMenu.Scepters.Find(x => x.ScepterID == selectScepter.ScepterId);
            if (scepterExcel != null) await AddScepter(scepterExcel);
            RogueActions.Remove(action.QueuePosition);
        }

        await UpdateMenu();

        await Player.SendPacket(
            new PacketHandleRogueCommonPendingActionScRsp(action.QueuePosition, selectScepter: true));
    }

    public async ValueTask AddScepter(RogueMagicScepterExcel excel, RogueCommonActionResultSourceType source = RogueCommonActionResultSourceType.Select)
    {
        var scepter = new RogueScepterInstance(excel);
        RogueScepters.Add(excel.ScepterID, scepter);

        await Player.SendPacket(new PacketSyncRogueCommonActionResultScNotify(RogueSubMode, scepter.ToGetInfo(source)));
    }

    public async ValueTask DressScepter(int scepterId, int slot, int unitUniqueId)
    {
        var scepter = RogueScepters.GetValueOrDefault(scepterId);
        if (scepter == null) return;

        var unit = RogueMagicUnits.GetValueOrDefault(unitUniqueId);
        if (unit == null) return;

        // add
        scepter.AddUnit(slot, unit);

        await Player.SendPacket(new PacketSyncRogueCommonActionResultScNotify(RogueSubMode, scepter.ToDressInfo(RogueCommonActionResultSourceType.None)));
    }

    #endregion

    #region MagicUnit

    public async ValueTask RollMagicUnit(int amount, int level, List<RogueMagicUnitCategoryEnum> categories)
    {
        var unitExcels = GameData.RogueMagicUnitData.Values.Where(x => !RogueMagicUnits.ContainsKey(x.MagicUnitID) && x.MagicUnitLevel == level && categories.Contains(x.MagicUnitCategory)).ToList();

        for (var i = 0; i < amount; i++)
        {
            var menu = new RogueMagicUnitSelectMenu(this);
            menu.RollMagicUnit(unitExcels);
            var action = menu.GetActionInstance();
            RogueActions.Add(action.QueuePosition, action);
        }

        await UpdateMenu();
    }

    public async ValueTask HandleMagicUnitSelect(RogueMagicGameUnit selectMagicUnit)
    {
        if (RogueActions.Count == 0) return;

        var action = RogueActions.First().Value;
        if (action.RogueMagicUnitSelectMenu != null)
        {
            var unitExcel = action.RogueMagicUnitSelectMenu.MagicUnits.Find(x => x.MagicUnitID == selectMagicUnit.MagicUnitId);
            if (unitExcel != null) await AddMagicUnit(unitExcel);
            RogueActions.Remove(action.QueuePosition);
        }

        await UpdateMenu();

        await Player.SendPacket(
            new PacketHandleRogueCommonPendingActionScRsp(action.QueuePosition, selectMagicUnit: true));
    }

    public async ValueTask AddMagicUnit(RogueMagicUnitExcel excel, RogueCommonActionResultSourceType source = RogueCommonActionResultSourceType.Select)
    {
        var unit = new RogueMagicUnitInstance(excel)
        {
            UniqueId = CurMagicUnitUniqueId++
        };
        RogueMagicUnits.Add(unit.UniqueId, unit);

        await Player.SendPacket(new PacketSyncRogueCommonActionResultScNotify(RogueSubMode, unit.ToGetInfo(source)));
    }

    #endregion

    #region Handlers

    public override void OnBattleStart(BattleInstance battle)
    {
        base.OnBattleStart(battle);
        battle.CustomLevel = AreaExcel.DifficultyIDList.RandomElement();
        battle.MagicInfo = new BattleRogueMagicInfo
        {
            ModifierContent = new IGEFNGNCKOG
            {
                OJIBOBNAIKH = 5
            },
            DetailInfo = new BattleRogueMagicDetailInfo
            {
                ILAPCDFJHNH = new BattleRogueMagicItemInfo
                {
                    BattleRoundCount = new BattleRogueMagicRoundCount(),
                    BattleScepterList = { RogueScepters.Select(x => x.Value.ToBattleScepterInfo()) }
                }
            }
        };
    }

    public override async ValueTask OnBattleEnd(BattleInstance battle, PVEBattleResultCsReq req)
    {
        if (battle.BattleEndStatus != BattleEndStatus.BattleEndWin)
        {
            // quit
            return;
        }

        if (CurLevel!.CurRoom!.RoomType == RogueMagicRoomTypeEnum.Boss)
        {
            await RollScepter(battle.Stages.Count, 1);
            await RollMagicUnit(battle.Stages.Count, 1, [RogueMagicUnitCategoryEnum.Ultra]);
        }
        else
        {
            await RollMagicUnit(battle.Stages.Count, 1, [RogueMagicUnitCategoryEnum.Common]);
        }
    }


    #endregion

    #region Serialization

    public RogueMagicCurInfo ToProto()
    {
        var proto = new RogueMagicCurInfo
        {
            Lineup = ToLineupInfo(),
            Level = ToLevelInfo(),
            ItemValue = ToGameItemValueInfo(),
            MiracleInfo = ToMiracleInfo(),
            GameDifficultyInfo = ToDifficultyInfo(),
            MagicItem = ToMagicItemInfo(),
            BasicInfo = ToCurAreaInfo(),
            JDMGJDBMHEJ = new LKJMDOAHGIN()
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

    public RogueMagicGameItemInfo ToMagicItemInfo()
    {
        var proto = new RogueMagicGameItemInfo
        {
            GameStyleType = (uint)StyleType
        };

        return proto;
    }

    public RogueMagicGameDifficultyInfo ToDifficultyInfo()
    {
        var proto = new RogueMagicGameDifficultyInfo
        {
            DifficultyIdList = { DifficultyCompExcels.Select(x => (uint)x.DifficultyCompID) }
        };

        return proto;
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


    public RogueMagicGameLevelInfo ToLevelInfo()
    {
        var proto = new RogueMagicGameLevelInfo
        {
            Status = LevelStatus,
            CurLevelIndex = (uint)(CurLevel?.CurRoomIndex ?? 0),
            Reason = RogueMagicSettleReason.None
        };

        foreach (var levelInstance in Levels.Values) proto.LevelInfoList.Add(levelInstance.ToProto());

        return proto;
    }

    public RogueMagicCurSceneInfo ToCurSceneInfo()
    {
        return new RogueMagicCurSceneInfo
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