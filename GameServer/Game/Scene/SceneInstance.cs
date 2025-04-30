using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.Data.Config.Scene;
using EggLink.DanhengServer.Data.Excel;
using EggLink.DanhengServer.Database.Avatar;
using EggLink.DanhengServer.Database.Player;
using EggLink.DanhengServer.Enums.Avatar;
using EggLink.DanhengServer.Enums.Scene;
using EggLink.DanhengServer.GameServer.Game.Activity.Loaders;
using EggLink.DanhengServer.GameServer.Game.Battle;
using EggLink.DanhengServer.GameServer.Game.Challenge;
using EggLink.DanhengServer.GameServer.Game.ChessRogue.Cell;
using EggLink.DanhengServer.GameServer.Game.Mission;
using EggLink.DanhengServer.GameServer.Game.Player;
using EggLink.DanhengServer.GameServer.Game.Rogue.Scene;
using EggLink.DanhengServer.GameServer.Game.RogueMagic.Scene;
using EggLink.DanhengServer.GameServer.Game.RogueTourn.Scene;
using EggLink.DanhengServer.GameServer.Game.Scene.Component;
using EggLink.DanhengServer.GameServer.Game.Scene.Entity;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.Scene;
using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Util;

namespace EggLink.DanhengServer.GameServer.Game.Scene;

public class SceneInstance
{
    #region Scene Details

    public EntityProp? GetNearestSpring(long minDistSq)
    {
        EntityProp? spring = null;
        long springDist = 0;

        foreach (var prop in HealingSprings)
        {
            var dist = Player.Data?.Pos?.GetFast2dDist(prop.Position) ?? 1000000;
            if (dist > minDistSq) continue;

            if (spring == null || dist < springDist)
            {
                spring = prop;
                springDist = dist;
            }
        }

        return spring;
    }

    #endregion

    #region Serialization

    public SceneInfo ToProto()
    {
        SceneInfo sceneInfo = new()
        {
            WorldId = (uint)(Excel.WorldID == 100 ? GameConstants.LAST_TRAIN_WORLD_ID : Excel.WorldID),
            GameModeType = (uint)GameModeType,
            PlaneId = (uint)PlaneId,
            FloorId = (uint)FloorId,
            EntryId = (uint)EntryId,
            SceneMissionInfo = new MissionStatusBySceneInfo(),
            DimensionId = (uint)(EntityLoader is StoryLineEntityLoader loader ? loader.DimensionId : 0),
            GameStoryLineId = (uint)(Player.StoryLineManager?.StoryLineData.CurStoryLineId ?? 0)
        };

        var playerGroupInfo = new SceneEntityGroupInfo(); // avatar group
        foreach (var avatar in AvatarInfo)
            playerGroupInfo.EntityList.Add(avatar.Value.ToProto());
        if (playerGroupInfo.EntityList.Count > 0)
        {
            if (LeaderEntityId == 0)
            {
                LeaderEntityId = AvatarInfo.Values.First().EntityId;
                sceneInfo.LeaderEntityId = (uint)LeaderEntityId;
            }
            else
            {
                sceneInfo.LeaderEntityId = (uint)LeaderEntityId;
            }
        }

        sceneInfo.EntityGroupList.Add(playerGroupInfo);

        List<SceneEntityGroupInfo> groups = []; // other groups

        // add entities to groups
        foreach (var entity in Entities)
        {
            if (entity.Value.GroupID == 0) continue;
            if (groups.FindIndex(x => x.GroupId == entity.Value.GroupID) == -1)
                groups.Add(new SceneEntityGroupInfo
                {
                    GroupId = (uint)entity.Value.GroupID
                });
            groups[groups.FindIndex(x => x.GroupId == entity.Value.GroupID)].EntityList.Add(entity.Value.ToProto());
        }

        foreach (var groupId in Groups) // Add for empty group
            if (groups.FindIndex(x => x.GroupId == groupId) == -1)
                groups.Add(new SceneEntityGroupInfo
                {
                    GroupId = (uint)groupId
                });

        foreach (var group in groups) sceneInfo.EntityGroupList.Add(group);

        // custom save data and floor saved data
        Player.SceneData!.CustomSaveData.TryGetValue(EntryId, out var data);

        if (data != null)
            foreach (var customData in data)
                sceneInfo.CustomDataList.Add(new CustomSaveData
                {
                    GroupId = (uint)customData.Key,
                    SaveData = customData.Value
                });

        Player.SceneData!.FloorSavedData.TryGetValue(FloorId, out var floorData);

        foreach (var value in FloorInfo?.FloorSavedValue ?? [])
            if (floorData != null && floorData.TryGetValue(value.Name, out var v))
                sceneInfo.FloorSavedData[value.Name] = v;
            else if (value.Name.Contains("_IsHidden"))
                sceneInfo.FloorSavedData[value.Name] = 0;
            else
                sceneInfo.FloorSavedData[value.Name] = value.DefaultValue;

        foreach (var value in floorData ?? [])
            sceneInfo.FloorSavedData[value.Key] = value.Value;

        // mission
        Player.MissionManager!.OnLoadScene(sceneInfo);

        // unlock section
        if (ConfigManager.Config.ServerOption.EnableMission)
        {
            Player.SceneData!.UnlockSectionIdList.TryGetValue(FloorId, out var unlockSectionList);
            if (unlockSectionList != null)
                foreach (var sectionId in unlockSectionList)
                    sceneInfo.LightenSectionList.Add((uint)sectionId);
        }
        else
        {
            GameData.GetFloorInfo(PlaneId, FloorId, out var floorInfo);
            sceneInfo.LightenSectionList.AddRange(floorInfo.MapSections.Select(x => (uint)x));
        }

        return sceneInfo;
    }

    #endregion

    #region Data

    public PlayerInstance Player;
    public MazePlaneExcel Excel;
    public FloorInfo? FloorInfo;
    public int FloorId;
    public int PlaneId;
    public int EntryId;

    public int LeaveEntryId;
    public int LastEntityId;
    public bool IsLoaded = false;

    public Dictionary<int, AvatarSceneInfo> AvatarInfo = [];
    public int LeaderEntityId;
    public Dictionary<int, IGameEntity> Entities = [];
    public List<int> Groups = [];
    public List<EntityProp> HealingSprings = [];

    public SceneEntityLoader? EntityLoader;

    public GameModeTypeEnum GameModeType;

    public EntitySummonUnit? SummonUnit;

    public SceneInstance(PlayerInstance player, MazePlaneExcel excel, int floorId, int entryId)
    {
        Player = player;
        Excel = excel;
        PlaneId = excel.PlaneID;
        FloorId = floorId;
        EntryId = entryId;
        LeaveEntryId = 0;

        System.Threading.Tasks.Task.Run(async () => { await SyncLineup(true); }).Wait();

        GameData.GetFloorInfo(PlaneId, FloorId, out FloorInfo);
        if (FloorInfo == null) return;

        GameModeType = (GameModeTypeEnum)excel.PlaneType;
        switch (Excel.PlaneType)
        {
            case PlaneTypeEnum.Rogue:
                if (Player.ChessRogueManager!.RogueInstance != null)
                {
                    EntityLoader = new ChessRogueEntityLoader(this);
                    GameModeType = GameModeTypeEnum.ChessRogue; // ChessRogue
                }
                else if (Player.RogueTournManager!.RogueTournInstance != null)
                {
                    EntityLoader = new RogueTournEntityLoader(this, Player);
                    GameModeType = GameModeTypeEnum.TournRogue; // TournRogue
                }
                else if (Player.RogueMagicManager!.RogueMagicInstance != null)
                {
                    EntityLoader = new RogueMagicEntityLoader(this, Player);
                    GameModeType = GameModeTypeEnum.MagicRogue; // MagicRogue
                }
                else
                {
                    EntityLoader = new RogueEntityLoader(this, Player);
                }

                break;
            case PlaneTypeEnum.Challenge:
                EntityLoader = new ChallengeEntityLoader(this, Player);
                break;
            case PlaneTypeEnum.TrialActivity:
                EntityLoader = new TrialActivityEntityLoader(this, Player);
                break;
            default:
                if (Player.StoryLineManager?.StoryLineData.CurStoryLineId != 0)
                    EntityLoader = new StoryLineEntityLoader(this);
                else
                    EntityLoader = new SceneEntityLoader(this);
                break;
        }

        System.Threading.Tasks.Task.Run(async () => { await EntityLoader.LoadEntity(); }).Wait();

        Player.TaskManager?.SceneTaskTrigger.TriggerFloor(PlaneId, FloorId);
    }

    #endregion

    #region Scene Actions

    public async ValueTask SyncLineup(bool notSendPacket = false)
    {
        var oldAvatarInfo = AvatarInfo.Values.ToList();
        AvatarInfo.Clear();
        var sendPacket = false;
        var addAvatar = new List<IGameEntity>();
        var removeAvatar = new List<IGameEntity>();
        var avatars = Player.LineupManager?.GetAvatarsFromCurTeam() ?? [];
        foreach (var sceneInfo in oldAvatarInfo)
        {
            if (avatars.FindIndex(x => x.AvatarInfo.BaseAvatarId == sceneInfo.AvatarInfo.BaseAvatarId) != -1)  // avatar still in team
            {
                AvatarInfo.Add(sceneInfo.EntityId, sceneInfo);
            }
            else  // avatar leave
            {
                removeAvatar.Add(sceneInfo);
                sendPacket = true;
            }
        }

        foreach (var avatar in avatars)  // check team avatar
        {
            if (AvatarInfo.ContainsKey(avatar.AvatarInfo.BaseAvatarId)) continue; // avatar already in team
            var avatarInfo = new AvatarSceneInfo(avatar.AvatarInfo, avatar.AvatarType, Player)
            {
                // assign entity id
                EntityId = ++LastEntityId
            };

            AvatarInfo.Add(avatarInfo.EntityId, avatarInfo);
            addAvatar.Add(avatarInfo);
            sendPacket = true;
        }

        var leaderAvatarId = Player.LineupManager?.GetCurLineup()?.LeaderAvatarId;
        var leaderAvatar = AvatarInfo.Values.FirstOrDefault(x => x.AvatarInfo.BaseAvatarId == leaderAvatarId);
        if (leaderAvatar == null) return;
        if (AvatarInfo.Count == 0) return;
        LeaderEntityId = leaderAvatar.EntityId;
        if (sendPacket && !notSendPacket)
            await Player.SendPacket(new PacketSceneGroupRefreshScNotify(Player, addAvatar, removeAvatar));

        foreach (var avatar in removeAvatar) Entities.Remove(avatar.EntityId);

        foreach (var avatar in addAvatar) Entities.Add(avatar.EntityId, avatar);
    }

    public void SyncGroupInfo()
    {
        EntityLoader?.SyncEntity();
    }

    public async ValueTask OnUseSkill(SceneCastSkillCsReq req)
    {
        foreach (var entity in Entities.Values.OfType<AvatarSceneInfo>())
        {
            if (!GameData.AvatarConfigData.TryGetValue(entity.AvatarInfo.AvatarId, out var excel)) continue;
            GameData.AdventureAbilityConfigListData.TryGetValue(excel.AdventurePlayerID, out var avatarAbility);
            if (avatarAbility == null) continue;
            foreach (var modifier in entity.Modifiers.ToArray())
            {
                // get modifier info
                if (!GameData.AdventureModifierData.TryGetValue(modifier, out var config)) continue;
                if (config.OnAfterLocalPlayerUseSkill.Count > 0)
                {
                    await Player.TaskManager!.AbilityLevelTask.TriggerTasks(avatarAbility,
                        config.OnAfterLocalPlayerUseSkill, entity, [], req);
                }
            }
        }
    }

    #endregion

    #region Entity Management

    public async ValueTask AddEntity(IGameEntity entity)
    {
        await AddEntity(entity, IsLoaded);
    }

    public async ValueTask AddEntity(IGameEntity entity, bool sendPacket)
    {
        if (entity.EntityId != 0) return;
        entity.EntityId = ++LastEntityId;

        Entities.Add(entity.EntityId, entity);
        if (sendPacket) await Player.SendPacket(new PacketSceneGroupRefreshScNotify(Player, entity));
    }

    public async ValueTask AddSummonUnitEntity(EntitySummonUnit entity)
    {
        if (entity.EntityId != 0) return;
        entity.EntityId = ++LastEntityId;
        // old

        foreach (var e in Entities.Values.Where(x => x is EntityMonster))
        {
            var monster = e as EntityMonster;
            monster!.IsInSummonUnit = false;
            List<SceneBuff> buffList = [.. monster.BuffList];
            foreach (var sceneBuff in buffList)
                if (sceneBuff.SummonUnitEntityId > 0)
                    // clear old buff
                    await monster.RemoveBuff(sceneBuff.BuffId);
        }

        await Player.SendPacket(new PacketSceneGroupRefreshScNotify(Player, entity, SummonUnit));
        SummonUnit = entity;
    }

    public async ValueTask RemoveEntity(IGameEntity monster)
    {
        await RemoveEntity(monster, IsLoaded);
    }

    public async ValueTask RemoveEntity(IGameEntity monster, bool sendPacket)
    {
        Entities.Remove(monster.EntityId);

        if (sendPacket) await Player.SendPacket(new PacketSceneGroupRefreshScNotify(Player, null, monster));
    }

    public List<T> GetEntitiesInGroup<T>(int groupID)
    {
        List<T> entities = [];
        foreach (var entity in Entities)
            if (entity.Value.GroupID == groupID && entity.Value is T t)
                entities.Add(t);
        return entities;
    }

    #endregion

    #region SummonUnit

    public async ValueTask<Retcode> TriggerSummonUnit(string triggerName, List<uint> targetIds)
    {
        if (SummonUnit == null) return Retcode.RetSceneEntityNotExist;

        // check trigger
        var trigger = SummonUnit.TriggerList.Find(x => x.TriggerName == triggerName);
        if (trigger == null) return Retcode.RetSceneUseSkillFail;

        await Player.SendPacket(
            new PacketRefreshTriggerByClientScNotify(triggerName, (uint)SummonUnit.EntityId, targetIds));
        // check target

        List<IGameEntity> targetEnter = [];
        List<IGameEntity> targetExit = [];
        foreach (var targetId in targetIds)
        {
            if (!Entities.TryGetValue((int)targetId, out var entity)) continue;
            EntityMonster? monster = null;
            EntityProp? prop = null;

            switch (entity)
            {
                case EntityMonster m:
                    monster = m;
                    break;
                case EntityProp p:
                    prop = p;
                    break;
            }

            if (monster != null)
            {
                if (!monster.IsAlive) continue;

                monster.IsInSummonUnit = true;
                targetEnter.Add(monster);
            }

            if (prop != null) targetEnter.Add(prop);
        }

        foreach (var gameEntity in Entities.Values)
        {
            if (gameEntity is not EntityMonster monster) continue;

            if (monster.IsInSummonUnit && !targetEnter.Contains(monster))
            {
                monster.IsInSummonUnit = false;
                targetExit.Add(monster);
            }
        }

        if (targetEnter.Count > 0)
        {
            // enter
            var config = trigger.OnTriggerEnter;

            Player.TaskManager!.SummonUnitLevelTask.TriggerTasks(config, targetEnter, SummonUnit);
        }

        if (targetExit.Count <= 0) return Retcode.RetSucc;
        {
            // enter
            var config = trigger.OnTriggerExit;

            Player.TaskManager!.SummonUnitLevelTask.TriggerTasks(config, targetExit, SummonUnit);
        }


        return Retcode.RetSucc;
    }

    public async ValueTask ClearSummonUnit()
    {
        if (SummonUnit == null) return;
        await Player.SendPacket(new PacketSceneGroupRefreshScNotify(Player, null, SummonUnit));

        SummonUnit = null;

        foreach (var entity in Entities.Values.Where(x => x is EntityMonster))
        {
            var monster = entity as EntityMonster;
            monster!.IsInSummonUnit = false;
            List<SceneBuff> buffList = [.. monster.BuffList];
            foreach (var sceneBuff in buffList)
                if (sceneBuff.SummonUnitEntityId > 0)
                    // clear old buff
                    await monster.RemoveBuff(sceneBuff.BuffId);
        }
    }

    public async ValueTask OnHeartBeat()
    {
        foreach (var gameEntity in Entities.Values.Clone().Where(x => x is EntityMonster).OfType<EntityMonster>())
        foreach (var sceneBuff in gameEntity.BuffList.Clone().Where(sceneBuff => sceneBuff.IsExpired()))
            await gameEntity.RemoveBuff(sceneBuff.BuffId);

        foreach (var gameEntity in AvatarInfo.Values.Clone())
        foreach (var sceneBuff in gameEntity.BuffList.Clone().Where(sceneBuff => sceneBuff.IsExpired()))
            await gameEntity.RemoveBuff(sceneBuff.BuffId);
        if (SummonUnit == null) return;
        var endTime = SummonUnit.CreateTimeMs + SummonUnit.LifeTimeMs;

        if (endTime < Extensions.GetUnixMs()) await ClearSummonUnit();
    }

    #endregion
}

public class AvatarSceneInfo : IGameEntity, IGameModifier
{
    public AvatarSceneInfo(BaseAvatarInfo avatarInfo, AvatarType avatarType, PlayerInstance player)
    {
        AvatarInfo = avatarInfo;
        AvatarType = avatarType;
        Player = player;

        // initialize enter ability
        if (!GameData.AvatarConfigData.TryGetValue(avatarInfo.AvatarId, out var excel)) return;
        var configInfo = GameData.CharacterConfigInfoData.GetValueOrDefault(excel.AdventurePlayerID);
        GameData.AdventureAbilityConfigListData.TryGetValue(excel.AdventurePlayerID, out var avatarAbility);
        if (configInfo == null || avatarAbility == null) return;

        foreach (var info in configInfo.SkillList.Where(x => x.UseType == SkillUseTypeEnum.Passive))
        {
            // cast ability
            var abilityStr = info.EntryAbility;
            // get ability
            var ability = avatarAbility.AbilityList.FirstOrDefault(x => x.Name == abilityStr);
            if (ability == null) continue;
            _ = Player.TaskManager!.AbilityLevelTask.TriggerTasks(avatarAbility, ability.OnStart, this, [],
                new SceneCastSkillCsReq());
        }
    }

    public BaseAvatarInfo AvatarInfo;
    public AvatarType AvatarType;
    public PlayerInstance Player;

    public List<SceneBuff> BuffList { get; set; } = [];

    public int EntityId { get; set; }

    public int GroupID { get; set; } = 0;

    public async ValueTask AddBuff(SceneBuff buff)
    {
        if (!GameData.MazeBuffData.TryGetValue(buff.BuffId * 10 + buff.BuffLevel, out var buffExcel)) return;

        var oldBuff = BuffList.Find(x => x.BuffId == buff.BuffId);
        if (oldBuff != null)
        {
            if (oldBuff.IsExpired())
            {
                BuffList.Remove(oldBuff);
                BuffList.Add(buff);
            }
            else
            {
                oldBuff.CreatedTime = Extensions.GetUnixMs();
                oldBuff.Duration = buff.Duration;

                await Player.SendPacket(new PacketSyncEntityBuffChangeListScNotify(this, oldBuff));
                await AddModifier(buffExcel.ModifierName);
                return;
            }
        }

        BuffList.Add(buff);
        await Player.SendPacket(new PacketSyncEntityBuffChangeListScNotify(this, buff));
        await AddModifier(buffExcel.ModifierName);
    }

    public async ValueTask ApplyBuff(BattleInstance instance)
    {
        if (BuffList.Count == 0) return;
        foreach (var buff in BuffList.Where(buff => !buff.IsExpired())) instance.Buffs.Add(new MazeBuff(buff));

        await Player.SendPacket(new PacketSyncEntityBuffChangeListScNotify(this, BuffList));

        foreach (var sceneBuff in BuffList)
        {
            if (!GameData.MazeBuffData.TryGetValue(sceneBuff.BuffId * 10 + sceneBuff.BuffLevel, out var buffExcel))
                continue;

            await RemoveModifier(buffExcel.ModifierName);
        }

        BuffList.Clear();
    }

    public SceneEntityInfo ToProto()
    {
        return new SceneEntityInfo
        {
            EntityId = (uint)EntityId,
            Motion = new MotionInfo
            {
                Pos = Player.Data.Pos?.ToProto() ?? new Vector(),
                Rot = Player.Data.Rot?.ToProto() ?? new Vector()
            },
            Actor = new SceneActorInfo
            {
                BaseAvatarId = (uint)AvatarInfo.BaseAvatarId,
                AvatarType = AvatarType
            }
        };
    }


    public List<string> Modifiers { get; set; } = [];

    public async ValueTask AddModifier(string modifierName)
    {
        if (Modifiers.Contains(modifierName)) return;

        GameData.AdventureModifierData.TryGetValue(modifierName, out var modifier);
        GameData.AdventureAbilityConfigListData.TryGetValue(AvatarInfo.AvatarId, out var avatarAbility);
        if (modifier == null || avatarAbility == null) return;

        await Player.TaskManager!.AbilityLevelTask.TriggerTasks(avatarAbility, modifier.OnCreate, this, [],
            new SceneCastSkillCsReq
            {
                TargetMotion = new MotionInfo
                {
                    Pos = Player.Data.Pos?.ToProto() ?? new Vector(),
                    Rot = Player.Data.Rot?.ToProto() ?? new Vector()
                }
            });

        Modifiers.Add(modifierName);
    }

    public async ValueTask RemoveModifier(string modifierName)
    {
        if (!Modifiers.Contains(modifierName)) return;

        GameData.AdventureModifierData.TryGetValue(modifierName, out var modifier);
        GameData.AdventureAbilityConfigListData.TryGetValue(AvatarInfo.AvatarId, out var avatarAbility);
        if (modifier == null || avatarAbility == null) return;

        await Player.TaskManager!.AbilityLevelTask.TriggerTasks(avatarAbility, modifier.OnDestroy, this, [],
            new SceneCastSkillCsReq());

        Modifiers.Remove(modifierName);
        ;
    }

    public async ValueTask RemoveBuff(int buffId)
    {
        if (!GameData.MazeBuffData.TryGetValue(buffId * 10 + 1, out var buffExcel)) return;

        var buff = BuffList.Find(x => x.BuffId == buffId);
        if (buff == null) return;

        BuffList.Remove(buff);
        await Player.SendPacket(new PacketSyncEntityBuffChangeListScNotify(this, [buff]));

        await RemoveModifier(buffExcel.ModifierName);
    }
}