using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.Database;
using EggLink.DanhengServer.Database.Player;
using EggLink.DanhengServer.Database.Scene;
using EggLink.DanhengServer.Database.Tutorial;
using EggLink.DanhengServer.Enums;
using EggLink.DanhengServer.Enums.Avatar;
using EggLink.DanhengServer.Enums.Mission;
using EggLink.DanhengServer.Enums.Scene;
using EggLink.DanhengServer.GameServer.Game.Activity;
using EggLink.DanhengServer.GameServer.Game.Avatar;
using EggLink.DanhengServer.GameServer.Game.Battle;
using EggLink.DanhengServer.GameServer.Game.Challenge;
using EggLink.DanhengServer.GameServer.Game.ChessRogue;
using EggLink.DanhengServer.GameServer.Game.Drop;
using EggLink.DanhengServer.GameServer.Game.Friend;
using EggLink.DanhengServer.GameServer.Game.Gacha;
using EggLink.DanhengServer.GameServer.Game.Inventory;
using EggLink.DanhengServer.GameServer.Game.Lineup;
using EggLink.DanhengServer.GameServer.Game.Mail;
using EggLink.DanhengServer.GameServer.Game.Message;
using EggLink.DanhengServer.GameServer.Game.Mission;
using EggLink.DanhengServer.GameServer.Game.Raid;
using EggLink.DanhengServer.GameServer.Game.Rogue;
using EggLink.DanhengServer.GameServer.Game.Scene;
using EggLink.DanhengServer.GameServer.Game.Scene.Entity;
using EggLink.DanhengServer.GameServer.Game.Shop;
using EggLink.DanhengServer.GameServer.Game.Task;
using EggLink.DanhengServer.GameServer.Server;
using EggLink.DanhengServer.GameServer.Server.Packet;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.Avatar;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.Lineup;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.Player;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.Scene;
using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Util;
using static EggLink.DanhengServer.GameServer.Plugin.Event.PluginEvent;

namespace EggLink.DanhengServer.GameServer.Game.Player;

public class PlayerInstance(PlayerData data)
{
    #region Managers

    public ActivityManager? ActivityManager { get; private set; }
    public AvatarManager? AvatarManager { get; private set; }
    public LineupManager? LineupManager { get; private set; }
    public InventoryManager? InventoryManager { get; private set; }
    public BattleManager? BattleManager { get; private set; }
    public BattleInstance? BattleInstance { get; set; }
    public MissionManager? MissionManager { get; private set; }
    public GachaManager? GachaManager { get; private set; }
    public MessageManager? MessageManager { get; private set; }
    public MailManager? MailManager { get; private set; }

    public RaidManager? RaidManager { get; private set; }
    public StoryLineManager? StoryLineManager { get; private set; }

    public FriendManager? FriendManager { get; private set; }
    public RogueManager? RogueManager { get; private set; }
    public ChessRogueManager? ChessRogueManager { get; private set; }
    public ShopService? ShopService { get; private set; }
    public ChallengeManager? ChallengeManager { get; private set; }

    public TaskManager? TaskManager { get; private set; }

    #endregion

    #region Datas

    public PlayerData Data { get; set; } = data;
    public PlayerUnlockData? PlayerUnlockData { get; private set; }
    public SceneData? SceneData { get; private set; }
    public HeartDialData? HeartDialData { get; private set; }
    public TutorialData? TutorialData { get; private set; }
    public TutorialGuideData? TutorialGuideData { get; private set; }
    public SceneInstance? SceneInstance { get; private set; }
    public int Uid { get; set; }
    public Connection? Connection { get; set; }
    public bool Initialized { get; set; }
    public bool IsNewPlayer { get; set; }
    public int NextBattleId { get; set; } = 0;
    public int ChargerNum { get; set; } = 0;

    #endregion

    #region Initializers

    public PlayerInstance(int uid) : this(new PlayerData { Uid = uid })
    {
        // new player
        IsNewPlayer = true;
        Data.NextStaminaRecover = Extensions.GetUnixSec() + GameConstants.STAMINA_RESERVE_RECOVERY_TIME;
        Data.Level = ConfigManager.Config.ServerOption.StartTrailblazerLevel;

        DatabaseHelper.SaveInstance(Data);


        var t = System.Threading.Tasks.Task.Run(async () =>
        {
            await InitialPlayerManager();

            await AddAvatar(8001);
            await AddAvatar(1001);
            if (ConfigManager.Config.ServerOption.EnableMission)
            {
                await LineupManager!.AddSpecialAvatarToCurTeam(10010050);
                await MissionManager!.AcceptMainMissionByCondition();
            }
            else
            {
                await LineupManager!.AddAvatarToCurTeam(8001);
                Data.CurrentGender = Gender.Man;
                Data.CurBasicType = 8001;
            }
        });
        t.Wait();

        Initialized = true;
    }

    private async ValueTask InitialPlayerManager()
    {
        Uid = Data.Uid;
        ActivityManager = new ActivityManager(this);
        AvatarManager = new AvatarManager(this);
        LineupManager = new LineupManager(this);
        InventoryManager = new InventoryManager(this);
        BattleManager = new BattleManager(this);
        MissionManager = new MissionManager(this);
        GachaManager = new GachaManager(this);
        MessageManager = new MessageManager(this);
        MailManager = new MailManager(this);
        FriendManager = new FriendManager(this);
        RogueManager = new RogueManager(this);
        ShopService = new ShopService(this);
        ChessRogueManager = new ChessRogueManager(this);
        ChallengeManager = new ChallengeManager(this);
        TaskManager = new TaskManager(this);
        RaidManager = new RaidManager(this);
        StoryLineManager = new StoryLineManager(this);

        PlayerUnlockData = InitializeDatabase<PlayerUnlockData>();
        SceneData = InitializeDatabase<SceneData>();
        HeartDialData = InitializeDatabase<HeartDialData>();
        TutorialData = InitializeDatabase<TutorialData>();
        TutorialGuideData = InitializeDatabase<TutorialGuideData>();

        Data.LastActiveTime = Extensions.GetUnixSec();
        DatabaseHelper.Instance?.UpdateInstance(Data);

        ChallengeManager.ResurrectInstance();
        await StoryLineManager.OnLogin();

        if (LineupManager!.GetCurLineup() != null) // null -> ignore(new player)
        {
            if (LineupManager!.GetCurLineup()!.IsExtraLineup() &&
                RaidManager!.RaidData.CurRaidId == 0 && StoryLineManager!.StoryLineData.CurStoryLineId == 0 &&
                ChallengeManager!.ChallengeInstance == null) // do not use extra lineup when login
            {
                LineupManager!.SetExtraLineup(ExtraLineupType.LineupNone, []);
                if (LineupManager!.GetCurLineup()!.IsExtraLineup()) await LineupManager!.SetCurLineup(0);
            }

            foreach (var lineup in LineupManager.LineupData.Lineups)
            {
                if (lineup.Value.BaseAvatars!.Count >= 5)
                    lineup.Value.BaseAvatars = lineup.Value.BaseAvatars.GetRange(0, 4);

                foreach (var avatar in lineup.Value.BaseAvatars!)
                    if (avatar.BaseAvatarId > 10000)
                    {
                        GameData.SpecialAvatarData.TryGetValue(avatar.BaseAvatarId, out var special);
                        if (special != null)
                        {
                            avatar.SpecialAvatarId = special.GetId();
                            avatar.BaseAvatarId = special.AvatarID;
                        }
                        else
                        {
                            GameData.SpecialAvatarData.TryGetValue(avatar.BaseAvatarId * 10 + Data.WorldLevel,
                                out special);
                            if (special != null)
                            {
                                avatar.SpecialAvatarId = special.GetId();
                                avatar.BaseAvatarId = special.AvatarID;
                            }
                        }
                    }
            }

            foreach (var avatar in LineupManager.GetCurLineup()!.BaseAvatars!)
            {
                var avatarData = AvatarManager.GetAvatar(avatar.BaseAvatarId);
                if (avatarData != null && avatarData.CurrentHp <= 0)
                    // revive
                    avatarData.CurrentHp = 2000;
            }
        }

        await LoadScene(Data.PlaneId, Data.FloorId, Data.EntryId, Data.Pos!, Data.Rot!, false);
        if (SceneInstance == null) await EnterScene(2000101, 0, false);
    }

    public T InitializeDatabase<T>() where T : class, new()
    {
        var instance = DatabaseHelper.Instance?.GetInstanceOrCreateNew<T>(Uid);
        return instance!;
    }

    #endregion

    #region Network

    public async ValueTask OnLogin()
    {
        if (!Initialized) await InitialPlayerManager();

        await SendPacket(new PacketStaminaInfoScNotify(this));

        InvokeOnPlayerLogin(this);
    }

    public void OnLogoutAsync()
    {
        InvokeOnPlayerLogout(this);
    }

    public async ValueTask SendPacket(BasePacket packet)
    {
        if (Connection?.IsOnline == true) await Connection.SendPacket(packet);
    }

    #endregion

    #region Actions

    public async ValueTask ChangeHeroBasicType(HeroBasicTypeEnum type)
    {
        var id = (int)((int)type + Data.CurrentGender - 1);
        if (Data.CurBasicType == id) return;
        Data.CurBasicType = id;
        AvatarManager!.GetHero()!.HeroId = id;
        AvatarManager!.GetHero()!.ValidateHero();
        AvatarManager!.GetHero()!.SetCurSp(0, LineupManager!.GetCurLineup()!.IsExtraLineup());
        await SendPacket(new PacketHeroBasicTypeChangedNotify(id));
        await SendPacket(new PacketPlayerSyncScNotify(AvatarManager!.GetHero()!));
    }

    public async ValueTask AddAvatar(int avatarId, bool sync = true, bool notify = true)
    {
        await AvatarManager!.AddAvatar(avatarId, sync, notify);
    }

    public async ValueTask SpendStamina(int staminaCost)
    {
        Data.Stamina -= staminaCost;
        await SendPacket(new PacketStaminaInfoScNotify(this));
    }

    public void OnAddExp()
    {
        GameData.PlayerLevelConfigData.TryGetValue(Data.Level, out var config);
        GameData.PlayerLevelConfigData.TryGetValue(Data.Level + 1, out var config2);
        if (config == null || config2 == null) return;
        var nextExp = config2.PlayerExp - config.PlayerExp;

        while (Data.Exp >= nextExp)
        {
            Data.Exp -= nextExp;
            Data.Level++;
            GameData.PlayerLevelConfigData.TryGetValue(Data.Level, out config);
            GameData.PlayerLevelConfigData.TryGetValue(Data.Level + 1, out config2);
            if (config == null || config2 == null) break;
            nextExp = config2.PlayerExp - config.PlayerExp;
        }

        OnLevelChange();
    }

    public void OnLevelChange()
    {
        if (!ConfigManager.Config.ServerOption.AutoUpgradeWorldLevel) return;
        var worldLevel = 0;
        foreach (var level in GameConstants.UpgradeWorldLevel)
            if (level <= Data.Level)
                worldLevel++;

        if (Data.WorldLevel != worldLevel) Data.WorldLevel = worldLevel;
    }

    public async ValueTask OnStaminaRecover()
    {
        var sendPacket = false;
        while (Data.NextStaminaRecover <= Extensions.GetUnixSec())
        {
            if (Data.Stamina >= GameConstants.MAX_STAMINA)
            {
                if (Data.StaminaReserve >= GameConstants.MAX_STAMINA_RESERVE) // needn't recover
                    break;
                Data.StaminaReserve = Math.Min(Data.StaminaReserve + 1, GameConstants.MAX_STAMINA_RESERVE);
            }
            else
            {
                Data.Stamina++;
            }

            Data.NextStaminaRecover = Data.NextStaminaRecover + (Data.Stamina >= GameConstants.MAX_STAMINA
                ? GameConstants.STAMINA_RESERVE_RECOVERY_TIME
                : GameConstants.STAMINA_RECOVERY_TIME);
            sendPacket = true;
        }

        if (sendPacket) await SendPacket(new PacketStaminaInfoScNotify(this));
    }

    public async ValueTask OnHeartBeat()
    {
        await OnStaminaRecover();

        InvokeOnPlayerHeartBeat(this);

        DatabaseHelper.ToSaveUidList.SafeAdd(Uid);
    }

    #endregion

    #region Scene Actions

    public async ValueTask OnMove()
    {
        if (SceneInstance != null)
        {
            var prop = SceneInstance.GetNearestSpring(25_000_000);

            var isInRange = prop != null;

            if (isInRange)
                if (LineupManager?.GetCurLineup()?.Heal(10000, true) == true)
                    await SendPacket(new PacketSyncLineupNotify(LineupManager.GetCurLineup()!));
        }
    }

    public async ValueTask<EntityProp?> InteractProp(int propEntityId, int interactId)
    {
        if (SceneInstance != null)
        {
            SceneInstance.Entities.TryGetValue(propEntityId, out var entity);
            if (entity == null) return null;
            if (entity is EntityProp prop)
            {
                GameData.InteractConfigData.TryGetValue(interactId, out var config);
                if (config == null || config.SrcState != prop.State) return prop;
                var oldState = prop.State;
                await prop.SetState(config.TargetState);
                var newState = prop.State;
                await SendPacket(new PacketGroupStateChangeScNotify(Data.EntryId, prop.GroupID, prop.State));

                switch (prop.Excel.PropType)
                {
                    case PropTypeEnum.PROP_TREASURE_CHEST:
                        if (oldState == PropStateEnum.ChestClosed && newState == PropStateEnum.ChestUsed)
                        {
                            // TODO: Filter treasure chest
                            var items = DropService.CalculateDropsFromProp();
                            await SceneInstance.Player.InventoryManager!.AddItems(items);
                        }

                        break;
                    case PropTypeEnum.PROP_DESTRUCT:
                        if (newState == PropStateEnum.Closed) await prop.SetState(PropStateEnum.Open);
                        break;
                    case PropTypeEnum.PROP_MAZE_PUZZLE:
                        if (newState == PropStateEnum.Closed || newState == PropStateEnum.Open)
                            foreach (var p in SceneInstance.GetEntitiesInGroup<EntityProp>(prop.GroupID))
                            {
                                if (p.Excel.PropType == PropTypeEnum.PROP_TREASURE_CHEST)
                                {
                                    await p.SetState(PropStateEnum.ChestUsed);
                                }
                                else if (p.Excel.PropType == PropTypeEnum.PROP_MAZE_PUZZLE)
                                {
                                    // Skip
                                }
                                else
                                {
                                    await p.SetState(PropStateEnum.Open);
                                }

                                await MissionManager!.OnPlayerInteractWithProp();
                            }

                        break;
                }

                // for door unlock
                if (prop.PropInfo.UnlockDoorID.Count > 0)
                    foreach (var id in prop.PropInfo.UnlockDoorID)
                    foreach (var p in SceneInstance.GetEntitiesInGroup<EntityProp>(id.Key))
                        if (id.Value.Contains(p.PropInfo.ID))
                        {
                            await p.SetState(PropStateEnum.Open);
                            await MissionManager!.OnPlayerInteractWithProp();
                        }

                // for mission
                await MissionManager!.OnPlayerInteractWithProp();

                // plane event
                InventoryManager!.HandlePlaneEvent(prop.PropInfo.EventID);

                // handle plugin event
                InvokeOnPlayerInteract(this, prop);

                var floorSavedKey = prop.PropInfo.Name.Replace("Controller_", "");
                var key = $"FSV_ML{floorSavedKey}{(config.TargetState == PropStateEnum.Open ? "Started" : "Complete")}";
                if (SceneInstance?.FloorInfo?.SavedValues.Find(x => x.Name == key) != null)
                {
                    // should save
                    var plane = SceneInstance.PlaneId;
                    var floor = SceneInstance.FloorId;
                    SceneData!.FloorSavedData.TryGetValue(floor, out var value);
                    if (value == null)
                    {
                        value = [];
                        SceneData.FloorSavedData[floor] = value;
                    }

                    value[key] = 1; // ParamString[2] is the key
                    await SendPacket(new PacketUpdateFloorSavedValueNotify(key, 1));

                    TaskManager?.SceneTaskTrigger.TriggerFloor(plane, floor);
                    MissionManager?.HandleFinishType(MissionFinishTypeEnum.FloorSavedValue);
                }

                return prop;
            }
        }

        return null;
    }

    public async ValueTask<bool> EnterScene(int entryId, int teleportId, bool sendPacket,
        ChangeStoryLineAction storyLineAction = ChangeStoryLineAction.None, int storyLineId = 0, bool mapTp = false)
    {
        if (storyLineId != StoryLineManager?.StoryLineData.CurStoryLineId)
        {
            StoryLineManager?.EnterStoryLine(storyLineId, entryId == 0); // entryId == 0 -> teleport
            mapTp = false; // do not use mapTp when enter story line
        }

        GameData.MapEntranceData.TryGetValue(entryId, out var entrance);
        if (entrance == null) return false;

        GameData.GetFloorInfo(entrance.PlaneID, entrance.FloorID, out var floorInfo);
        if (floorInfo == null) return false;

        var StartGroup = entrance.StartGroupID;
        var StartAnchor = entrance.StartAnchorID;

        if (teleportId != 0)
        {
            floorInfo.CachedTeleports.TryGetValue(teleportId, out var teleport);
            if (teleport != null)
            {
                StartGroup = teleport.AnchorGroupID;
                StartAnchor = teleport.AnchorID;
            }
        }
        else if (StartAnchor == 0)
        {
            StartGroup = floorInfo.StartGroupID;
            StartAnchor = floorInfo.StartAnchorID;
        }

        var anchor = floorInfo.GetAnchorInfo(StartGroup, StartAnchor);

        MissionManager?.HandleFinishType(MissionFinishTypeEnum.EnterMapByEntrance, entryId);

        var beforeEntryId = Data.EntryId;

        await LoadScene(entrance.PlaneID, entrance.FloorID, entryId, anchor!.ToPositionProto(),
            anchor.ToRotationProto(), sendPacket, storyLineAction, mapTp);

        var afterEntryId = Data.EntryId;

        return beforeEntryId != afterEntryId; // return true if entryId changed
    }

    public async ValueTask EnterMissionScene(int entranceId, int anchorGroupId, int anchorId, bool sendPacket,
        ChangeStoryLineAction storyLineAction = ChangeStoryLineAction.None)
    {
        GameData.MapEntranceData.TryGetValue(entranceId, out var entrance);
        if (entrance == null) return;

        GameData.GetFloorInfo(entrance.PlaneID, entrance.FloorID, out var floorInfo);
        if (floorInfo == null) return;

        var StartGroup = anchorGroupId == 0 ? entrance.StartGroupID : anchorGroupId;
        var StartAnchor = anchorId == 0 ? entrance.StartAnchorID : anchorId;

        if (StartAnchor == 0)
        {
            StartGroup = floorInfo.StartGroupID;
            StartAnchor = floorInfo.StartAnchorID;
        }

        var anchor = floorInfo.GetAnchorInfo(StartGroup, StartAnchor);

        await LoadScene(entrance.PlaneID, entrance.FloorID, entranceId, anchor!.ToPositionProto(),
            anchor.ToRotationProto(), sendPacket, storyLineAction);
    }

    public async ValueTask MoveTo(Position position)
    {
        Data.Pos = position;
        await SendPacket(new PacketSceneEntityMoveScNotify(this));
    }

    public void MoveTo(EntityMotion motion)
    {
        Data.Pos = motion.Motion.Pos.ToPosition();
        Data.Rot = motion.Motion.Rot.ToPosition();
    }

    public async ValueTask MoveTo(Position pos, Position rot)
    {
        Data.Pos = pos;
        Data.Rot = rot;
        await SendPacket(new PacketSceneEntityMoveScNotify(this));
    }

    public async ValueTask LoadScene(int planeId, int floorId, int entryId, Position pos, Position rot, bool sendPacket,
        ChangeStoryLineAction storyLineAction = ChangeStoryLineAction.None, bool mapTp = false)
    {
        GameData.MazePlaneData.TryGetValue(planeId, out var plane);
        if (plane == null) return;

        if (plane.PlaneType == PlaneTypeEnum.Rogue && RogueManager!.GetRogueInstance() == null)
        {
            await EnterScene(801120102, 0, sendPacket);
            return;
        }

        if (plane.PlaneType == PlaneTypeEnum.Raid && RaidManager!.RaidData.CurRaidId == 0)
        {
            await EnterScene(2000101, 0, sendPacket);
            return;
        }

        if (plane.PlaneType == PlaneTypeEnum.Challenge && ChallengeManager!.ChallengeInstance == null)
        {
            await EnterScene(100000103, 0, sendPacket);
            return;
        }

        // TODO: Sanify check
        Data.Pos = pos;
        Data.Rot = rot;
        var notSendMove = true;
        SceneInstance instance = new(this, plane, floorId, entryId);
        if (planeId != Data.PlaneId || floorId != Data.FloorId || entryId != Data.EntryId)
        {
            Data.PlaneId = planeId;
            Data.FloorId = floorId;
            Data.EntryId = entryId;
        }
        else if (StoryLineManager?.StoryLineData.CurStoryLineId == 0 &&
                 mapTp) // only send move packet when not in story line and mapTp
        {
            notSendMove = false;
        }

        SceneInstance = instance;

        MissionManager?.OnPlayerChangeScene();

        Connection?.SendPacket(CmdIds.SyncServerSceneChangeNotify);
        if (sendPacket && notSendMove)
            await SendPacket(new PacketEnterSceneByServerScNotify(instance, storyLineAction));
        else if (sendPacket && !notSendMove) // send move packet
            await SendPacket(new PacketSceneEntityMoveScNotify(this));

        MissionManager?.HandleFinishType(MissionFinishTypeEnum.EnterFloor);
        MissionManager?.HandleFinishType(MissionFinishTypeEnum.EnterPlane);
        MissionManager?.HandleFinishType(MissionFinishTypeEnum.NotInFloor);
        MissionManager?.HandleFinishType(MissionFinishTypeEnum.NotInPlane);
    }

    public ScenePropData? GetScenePropData(int floorId, int groupId, int propId)
    {
        if (SceneData != null)
            if (SceneData.ScenePropData.TryGetValue(floorId, out var floorData))
                if (floorData.TryGetValue(groupId, out var groupData))
                {
                    var propData = groupData.Find(x => x.PropId == propId);
                    return propData;
                }

        return null;
    }

    public void SetScenePropData(int floorId, int groupId, int propId, PropStateEnum state)
    {
        if (SceneData != null)
        {
            if (!SceneData.ScenePropData.TryGetValue(floorId, out var floorData))
            {
                floorData = [];
                SceneData.ScenePropData.Add(floorId, floorData);
            }

            if (!floorData.TryGetValue(groupId, out var groupData))
            {
                groupData = [];
                floorData.Add(groupId, groupData);
            }

            var propData = groupData.Find(x => x.PropId == propId); // find prop data
            if (propData == null)
            {
                propData = new ScenePropData
                {
                    PropId = propId,
                    State = state
                };
                groupData.Add(propData);
            }
            else
            {
                propData.State = state;
            }
        }
    }

    public void EnterSection(int sectionId)
    {
        if (SceneInstance != null)
        {
            SceneData!.UnlockSectionIdList.TryGetValue(SceneInstance.FloorId, out var unlockList);
            if (unlockList == null)
            {
                unlockList = [sectionId];
                SceneData.UnlockSectionIdList.Add(SceneInstance.FloorId, unlockList);
            }
            else
            {
                SceneData.UnlockSectionIdList[SceneInstance.FloorId].Add(sectionId);
            }
        }
    }

    public void SetCustomSaveData(int entryId, int groupId, string data)
    {
        if (SceneData != null)
        {
            if (!SceneData.CustomSaveData.TryGetValue(entryId, out var entryData))
            {
                entryData = [];
                SceneData.CustomSaveData.Add(entryId, entryData);
            }

            entryData[groupId] = data;
        }
    }

    public async ValueTask ForceQuitBattle()
    {
        if (BattleInstance != null)
        {
            BattleInstance = null;
            await Connection!.SendPacket(CmdIds.QuitBattleScNotify);
        }
    }

    #endregion

    #region Serialization

    public PlayerBasicInfo ToProto()
    {
        return Data.ToProto();
    }

    public PlayerSimpleInfo ToSimpleProto()
    {
        return Data.ToSimpleProto(FriendOnlineStatus.Online);
    }

    #endregion
}