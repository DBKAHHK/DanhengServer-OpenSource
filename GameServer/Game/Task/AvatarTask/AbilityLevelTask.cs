using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.Data.Config;
using EggLink.DanhengServer.Data.Config.Task;
using EggLink.DanhengServer.GameServer.Game.Battle;
using EggLink.DanhengServer.GameServer.Game.Player;
using EggLink.DanhengServer.GameServer.Game.Scene;
using EggLink.DanhengServer.GameServer.Game.Scene.Entity;
using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Util;

namespace EggLink.DanhengServer.GameServer.Game.Task.AvatarTask;

public class AbilityLevelTask(PlayerInstance player)
{
    public PlayerInstance Player { get; set; } = player;

    #region Manage

    public async ValueTask<AbilityLevelResult> TriggerTasks(AdventureAbilityConfigListInfo abilities, List<TaskConfigInfo> tasks, IGameEntity casterEntity, List<IGameEntity> targetEntities, SceneCastSkillCsReq req)
    {
        BattleInstance? instance = null;
        List<HitMonsterInstance> battleInfos = [];
        foreach (var task in tasks)
        {
            try
            {
                var res = await TriggerTask(new AbilityLevelParam(abilities, task, casterEntity, targetEntities, req));
                if (res.BattleInfos != null)
                {
                    battleInfos.AddRange(res.BattleInfos);
                }

                if (res.Instance != null)
                {
                    instance = res.Instance;
                }
            }
            catch (Exception e)
            {
                Logger.GetByClassName().Error("An error occured, ", e);
            }
        }

        return new AbilityLevelResult(instance, battleInfos);
    }

    public async ValueTask<AbilityLevelResult> TriggerTask(AbilityLevelParam param)
    {
        try
        {
            var methodName = param.Act.Type.Replace("RPG.GameCore.", "");

            var method = GetType().GetMethod(methodName);
            if (method != null)
            {
                var res = method.Invoke(this, [param]);
                if (res is AbilityLevelResult result)
                {
                    return result;
                }

                if (res is ValueTask<AbilityLevelResult> valueTask)
                {
                    return await valueTask;
                }
            }
        }
        catch
        {
            // ignored
        }

        return new AbilityLevelResult();
    }

    #endregion

    #region Task

    public async ValueTask<AbilityLevelResult> PredicateTaskList(AbilityLevelParam param)
    {
        BattleInstance? instance = null;
        List<HitMonsterInstance> battleInfos = [];

        if (param.Act is PredicateTaskList predicateTaskList)
        {
            // handle predicateCondition
            var methodName = predicateTaskList.Predicate.Type.Replace("RPG.GameCore.", "");
            var method = GetType().GetMethod(methodName);
            if (method != null)
            {
                var resp = method.Invoke(this, [predicateTaskList.Predicate, param.CasterEntity, param.TargetEntities]);
                if (resp is true)
                {
                    foreach (var task in predicateTaskList.SuccessTaskList)
                    {
                        var result = await TriggerTask(param with { Act = task });
                        if (result.BattleInfos != null)
                        {
                            battleInfos.AddRange(result.BattleInfos);
                        }

                        if (result.Instance != null)
                        {
                            instance = result.Instance;
                        }
                    }
                }
                else
                {
                    foreach (var task in predicateTaskList.FailedTaskList)
                    {
                        var result = await TriggerTask(param with { Act = task });
                        if (result.BattleInfos != null)
                        {
                            battleInfos.AddRange(result.BattleInfos);
                        }

                        if (result.Instance != null)
                        {
                            instance = result.Instance;
                        }
                    }
                }
            }
            else
            {
                foreach (var task in predicateTaskList.FailedTaskList)
                {
                    var result = await TriggerTask(param with { Act = task });
                    if (result.BattleInfos != null)
                    {
                        battleInfos.AddRange(result.BattleInfos);
                    }

                    if (result.Instance != null)
                    {
                        instance = result.Instance;
                    }
                }
            }
        }
        return new AbilityLevelResult(instance, battleInfos);
    }

    public async ValueTask<AbilityLevelResult> AdventureTriggerAttack(AbilityLevelParam param)
    {
        BattleInstance? instance = null;
        List<HitMonsterInstance> battleInfos = [];

        if (param.Act is AdventureTriggerAttack adventureTriggerAttack)
        {
            var methodName = adventureTriggerAttack.AttackTargetType.Type.Replace("RPG.GameCore.", "");
            var method = GetType().GetMethod(methodName);
            if (method != null)
            {
                var resp = method.Invoke(this, [adventureTriggerAttack.AttackTargetType, param.CasterEntity, param.TargetEntities]);
                if (resp is List<IGameEntity> target)
                {
                    foreach (var task in adventureTriggerAttack.OnAttack)
                    {
                        var result = await TriggerTask(param with { Act = task });
                        if (result.BattleInfos != null)
                        {
                            battleInfos.AddRange(result.BattleInfos);
                        }
                    }

                    if (target.Count > 0 && adventureTriggerAttack.TriggerBattle)
                    {
                        foreach (var task in adventureTriggerAttack.OnBattle)
                        {
                            var result = await TriggerTask(param with { Act = task });
                            if (result.BattleInfos != null)
                            {
                                battleInfos.AddRange(result.BattleInfos);
                            }
                        }

                        foreach (var entity in param.TargetEntities)
                        {
                            var type = MonsterBattleType.TriggerBattle;
                            if (entity is EntityMonster { IsAlive: false })
                                type = MonsterBattleType.DirectDieSkipBattle;

                            battleInfos.Add(new HitMonsterInstance(entity.EntityID, type));
                        }

                        instance = await Player.BattleManager!.StartBattle(param.CasterEntity, param.TargetEntities, param.Request.SkillIndex == 1);
                    }
                }
            }
        }

        return new AbilityLevelResult(instance, battleInfos);
    }

    public async ValueTask<AbilityLevelResult> AddMazeBuff(AbilityLevelParam param)
    {
        BattleInstance? instance = null;
        List<HitMonsterInstance> battleInfos = [];

        if (param.Act is AddMazeBuff addMazeBuff)
        {
            var methodName = addMazeBuff.TargetType.Type.Replace("RPG.GameCore.", "");
            var method = GetType().GetMethod(methodName);
            if (method != null)
            {
                var resp = method.Invoke(this,
                    [addMazeBuff.TargetType, param.CasterEntity, param.TargetEntities]);
                if (resp is List<IGameEntity> target)
                {
                    foreach (var entity in target)
                    {
                        await entity.AddBuff(new SceneBuff(addMazeBuff.ID, 1, (param.CasterEntity as AvatarSceneInfo)?.AvatarInfo.GetAvatarId() ?? 0,
                            addMazeBuff.LifeTime.FixedValue.Value < -1 ? 20 : -1));

                        await AddAdventureModifier(param with
                        {
                            Act = new AddAdventureModifier
                            {
                                ModifierName = GameData.MazeBuffData[addMazeBuff.ID * 10 + 1].ModifierName
                            }
                        });
                    }
                }
            }
        }

        return new AbilityLevelResult(instance, battleInfos);
    }

    public async ValueTask<AbilityLevelResult> AdventureFireProjectile(AbilityLevelParam param)
    {
        BattleInstance? instance = null;
        List<HitMonsterInstance> battleInfos = [];

        if (param.Act is AdventureFireProjectile adventureFireProjectile)
        {
            foreach (var task in adventureFireProjectile.OnProjectileHit)
            {
                var result = await TriggerTask(param with { Act = task });
                if (result.BattleInfos != null)
                {
                    battleInfos.AddRange(result.BattleInfos);
                }

                if (result.Instance != null)
                    instance = result.Instance;
            }

            foreach (var task in adventureFireProjectile.OnProjectileLifetimeFinish)
            {
                var result = await TriggerTask(param with { Act = task });
                if (result.BattleInfos != null)
                {
                    battleInfos.AddRange(result.BattleInfos);
                }

                if (result.Instance != null)
                    instance = result.Instance;
            }
        }

        return new AbilityLevelResult(instance, battleInfos);
    }

    public async ValueTask<AbilityLevelResult> CreateSummonUnit(AbilityLevelParam param)
    {
        if (param.Act is CreateSummonUnit createSummonUnit)
        {
            if (!GameData.SummonUnitDataData.TryGetValue(createSummonUnit.SummonUnitID, out var excel))
                return new AbilityLevelResult();

            var unit = new EntitySummonUnit
            {
                EntityID = 0,
                CreateAvatarEntityId = param.CasterEntity.EntityID,
                AttachEntityId = excel.ConfigInfo?.AttachPoint == "Origin" ? param.CasterEntity.EntityID : 0,
                SummonUnitId = excel.ID,
                CreateAvatarId = (param.CasterEntity as AvatarSceneInfo)?.AvatarInfo.GetAvatarId() ?? 0,
                LifeTimeMs = 20000,
                TriggerList = excel.ConfigInfo?.TriggerConfig.CustomTriggers ?? [],
                Motion = param.Request.TargetMotion
            };

            await Player.SceneInstance!.AddSummonUnitEntity(unit);
        }

        return new AbilityLevelResult();
    }

    public async ValueTask<AbilityLevelResult> DestroySummonUnit(AbilityLevelParam param)
    {
        if (param.Act is CreateSummonUnit createSummonUnit)
        {
            await Player.SceneInstance!.ClearSummonUnit(); // TODO
        }

        return new AbilityLevelResult();
    }

    public async ValueTask<AbilityLevelResult> AddAdventureModifier(AbilityLevelParam param)
    {
        if (param.Act is AddAdventureModifier addAdventureModifier)
        {
            param.AdventureAbility.GlobalModifiers.TryGetValue(addAdventureModifier.ModifierName, out var modifier);
            if (modifier == null) return new AbilityLevelResult();

            foreach (var task in modifier.OnCreate)
            {
                await TriggerTask(param with { Act = task });
            }
        }

        return new AbilityLevelResult();
    }

    public async ValueTask<AbilityLevelResult> RemoveAdventureModifier(AbilityLevelParam param)
    {
        if (param.Act is RemoveAdventureModifier removeAdventureModifier)
        {
            param.AdventureAbility.GlobalModifiers.TryGetValue(removeAdventureModifier.ModifierName, out var modifier);
            if (modifier == null) return new AbilityLevelResult();

            foreach (var task in modifier.OnDestroy)
            {
                await TriggerTask(param with { Act = task });
            }
        }

        return new AbilityLevelResult();
    }

    #endregion

    #region Selector

    public List<IGameEntity> TargetAlias(TargetEvaluator selector, IGameEntity casterEntity, List<IGameEntity> targetEntities)
    {
        if (selector is TargetAlias target)
        {
            if (target.Alias == "AllEnemy")
            {
                return targetEntities;
            }

            if (target.Alias == "Caster")
            {
                return [casterEntity];
            }

            if (target.Alias == "AbilityTargetEntity")
            {
                return targetEntities;
            }

            if (target.Alias == "ParamEntity")
            {
                return targetEntities;
            }
        }

        return [];
    }

    #endregion
}

public record AbilityLevelResult(BattleInstance? Instance = null, List<HitMonsterInstance>? BattleInfos = null);

public record AbilityLevelParam(AdventureAbilityConfigListInfo AdventureAbility, TaskConfigInfo Act, IGameEntity CasterEntity, List<IGameEntity> TargetEntities, SceneCastSkillCsReq Request);