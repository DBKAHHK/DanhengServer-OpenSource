using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.Data.Config;
using EggLink.DanhengServer.Data.Config.Task;
using EggLink.DanhengServer.Enums.RogueMagic;
using EggLink.DanhengServer.Enums.Scene;
using EggLink.DanhengServer.GameServer.Game.Battle;
using EggLink.DanhengServer.GameServer.Game.Player;
using EggLink.DanhengServer.GameServer.Game.RogueMagic;
using EggLink.DanhengServer.GameServer.Game.Scene;
using EggLink.DanhengServer.GameServer.Game.Scene.Component;
using EggLink.DanhengServer.GameServer.Game.Scene.Entity;
using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Util;

namespace EggLink.DanhengServer.GameServer.Game.Task.AvatarTask;

public class AbilityLevelTask(PlayerInstance player)
{
    public PlayerInstance Player { get; set; } = player;

    #region Selector

    public List<IGameEntity> TargetAlias(TargetEvaluator selector, IGameEntity casterEntity,
        List<IGameEntity> targetEntities)
    {
        if (selector is TargetAlias target)
        {
            return target.Alias switch
            {
                "Caster" or "ModifierOwnerEntity" => [casterEntity],
                "ParamEntity" or "AllEnemy" or "AbilityTargetEntity" => targetEntities,
                _ => targetEntities,
            };
        }

        return [];
    }

    #endregion

    #region Manage

    public async ValueTask<AbilityLevelResult> TriggerTasks(AdventureAbilityConfigListInfo abilities,
        List<TaskConfigInfo> tasks, IGameEntity casterEntity, List<IGameEntity> targetEntities, SceneCastSkillCsReq req)
    {
        BattleInstance? instance = null;
        List<HitMonsterInstance> battleInfos = [];
        foreach (var task in tasks)
            try
            {
                var res = await TriggerTask(new AbilityLevelParam(abilities, task, casterEntity, targetEntities, req));
                if (res.BattleInfos != null) battleInfos.AddRange(res.BattleInfos);

                if (res.Instance != null) instance = res.Instance;
            }
            catch (Exception e)
            {
                Logger.GetByClassName().Error("An error occured, ", e);
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
                if (res is AbilityLevelResult result) return result;

                if (res is ValueTask<AbilityLevelResult> valueTask) return await valueTask;
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
                var resp = method.Invoke(this, [param with { Act = predicateTaskList.Predicate }]);
                if (resp is true)
                    foreach (var task in predicateTaskList.SuccessTaskList)
                    {
                        var result = await TriggerTask(param with { Act = task });
                        if (result.BattleInfos != null) battleInfos.AddRange(result.BattleInfos);

                        if (result.Instance != null) instance = result.Instance;
                    }
                else
                    foreach (var task in predicateTaskList.FailedTaskList)
                    {
                        var result = await TriggerTask(param with { Act = task });
                        if (result.BattleInfos != null) battleInfos.AddRange(result.BattleInfos);

                        if (result.Instance != null) instance = result.Instance;
                    }
            }
            else
            {
                foreach (var task in predicateTaskList.SuccessTaskList)
                {
                    var result = await TriggerTask(param with { Act = task });
                    if (result.BattleInfos != null) battleInfos.AddRange(result.BattleInfos);

                    if (result.Instance != null) instance = result.Instance;
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
                var resp = method.Invoke(this,
                    [adventureTriggerAttack.AttackTargetType, param.CasterEntity, param.TargetEntities]);
                if (resp is List<IGameEntity> target)
                {
                    foreach (var task in adventureTriggerAttack.OnAttack)
                    {
                        var result = await TriggerTask(param with { Act = task });
                        if (result.BattleInfos != null) battleInfos.AddRange(result.BattleInfos);
                    }

                    if (target.Count > 0 && adventureTriggerAttack.TriggerBattle)
                    {
                        foreach (var task in adventureTriggerAttack.OnBattle)
                        {
                            var result = await TriggerTask(param with { Act = task });
                            if (result.BattleInfos != null) battleInfos.AddRange(result.BattleInfos);
                        }

                        foreach (var entity in param.TargetEntities)
                        {
                            var type = MonsterBattleType.TriggerBattle;
                            if (entity is EntityMonster { IsAlive: false })
                                type = MonsterBattleType.DirectDieSkipBattle;

                            battleInfos.Add(new HitMonsterInstance(entity.EntityID, type));
                        }

                        instance = await Player.BattleManager!.StartBattle(param.CasterEntity, param.TargetEntities,
                            param.Request.SkillIndex == 1);
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
                    foreach (var entity in target)
                        await entity.AddBuff(new SceneBuff(addMazeBuff.ID, 1,
                            (param.CasterEntity as AvatarSceneInfo)?.AvatarInfo.GetAvatarId() ?? 0,
                            addMazeBuff.LifeTime.FixedValue.Value < -1 ? 20 : -1));
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
                if (result.BattleInfos != null) battleInfos.AddRange(result.BattleInfos);

                if (result.Instance != null)
                    instance = result.Instance;
            }

            foreach (var task in adventureFireProjectile.OnProjectileLifetimeFinish)
            {
                var result = await TriggerTask(param with { Act = task });
                if (result.BattleInfos != null) battleInfos.AddRange(result.BattleInfos);

                if (result.Instance != null)
                    instance = result.Instance;
            }
        }

        return new AbilityLevelResult(instance, battleInfos);
    }

    public async ValueTask<AbilityLevelResult> NewAdventureFireProjectile(AbilityLevelParam param)
    {
        BattleInstance? instance = null;
        List<HitMonsterInstance> battleInfos = [];

        if (param.Act is AdventureFireProjectile adventureFireProjectile)
        {
            foreach (var task in adventureFireProjectile.OnProjectileHit)
            {
                var result = await TriggerTask(param with { Act = task });
                if (result.BattleInfos != null) battleInfos.AddRange(result.BattleInfos);

                if (result.Instance != null)
                    instance = result.Instance;
            }

            foreach (var task in adventureFireProjectile.OnProjectileLifetimeFinish)
            {
                var result = await TriggerTask(param with { Act = task });
                if (result.BattleInfos != null) battleInfos.AddRange(result.BattleInfos);

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
        if (param.Act is CreateSummonUnit createSummonUnit) await Player.SceneInstance!.ClearSummonUnit(); // TODO

        return new AbilityLevelResult();
    }

    public async ValueTask<AbilityLevelResult> AddAdventureModifier(AbilityLevelParam param)
    {
        if (param.Act is AddAdventureModifier addAdventureModifier)
        {
            GameData.AdventureModifierData.TryGetValue(addAdventureModifier.ModifierName, out var modifier);
            if (modifier == null) return new AbilityLevelResult();

            if (param.CasterEntity is IGameModifier mod) await mod.AddModifier(addAdventureModifier.ModifierName);
        }

        return new AbilityLevelResult();
    }

    public async ValueTask<AbilityLevelResult> RemoveAdventureModifier(AbilityLevelParam param)
    {
        if (param.Act is RemoveAdventureModifier removeAdventureModifier)
        {
            GameData.AdventureModifierData.TryGetValue(removeAdventureModifier.ModifierName, out var modifier);
            if (modifier == null) return new AbilityLevelResult();

            if (param.CasterEntity is IGameModifier mod) await mod.RemoveModifier(removeAdventureModifier.ModifierName);
        }

        return new AbilityLevelResult();
    }

    public async ValueTask AdventureSetAttackTargetMonsterDie(AbilityLevelParam param)
    {
        foreach (var targetEntity in param.TargetEntities)
        {
            if (targetEntity is not EntityMonster monster) continue;

            if (monster.MonsterData.Rank < MonsterRankEnum.Elite)
            {
                await monster.Kill();

                await monster.Scene.Player.LineupManager!.CostMp(1, param.Request.CastEntityId);
                var instance = monster.Scene.Player.RogueManager!.GetRogueInstance();
                switch (instance)
                {
                    case null:
                        continue;
                    case RogueMagicInstance magic:
                        await magic.RollMagicUnit(1, 1, [RogueMagicUnitCategoryEnum.Common]);
                        break;
                    default:
                        await instance.RollBuff(1);
                        break;
                }

                await instance.GainMoney(Random.Shared.Next(20, 60));
            }
        }
    }

    #endregion

    #region Predicate

    public bool ByAllowInstantKill(AbilityLevelParam param)
    {
        foreach (var targetEntity in param.TargetEntities)
            if (targetEntity is EntityMonster monster)
                if (monster.MonsterData.Rank < MonsterRankEnum.Elite)
                    return true;

        return false;
    }

    public bool ByIsContainAdventureModifier(AbilityLevelParam param)
    {
        if (param.Act is ByIsContainAdventureModifier byIsContain)
        {
            // get target
            var result = false;
            var methodName = byIsContain.TargetType.Type.Replace("RPG.GameCore.", "");
            var method = GetType().GetMethod(methodName);
            if (method != null)
            {
                var resp = method.Invoke(this,
                    [byIsContain.TargetType, param.CasterEntity, param.TargetEntities]);

                if (resp is List<IGameEntity> target)
                    foreach (var entity in target)
                    {
                        if (entity is not IGameModifier modifier) continue;
                        if (modifier.Modifiers.Contains(byIsContain.ModifierName))
                        {
                            result = true;
                            break;
                        }
                    }
            }

            return result;
        }

        return false;
    }

    public bool AdventureByInMotionState(AbilityLevelParam param)
    {
        return true;
    }

    #endregion
}

public record AbilityLevelResult(BattleInstance? Instance = null, List<HitMonsterInstance>? BattleInfos = null);

public record AbilityLevelParam(
    AdventureAbilityConfigListInfo AdventureAbility,
    TaskConfigInfo Act,
    IGameEntity CasterEntity,
    List<IGameEntity> TargetEntities,
    SceneCastSkillCsReq Request);