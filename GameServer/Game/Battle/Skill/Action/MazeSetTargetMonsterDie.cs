using EggLink.DanhengServer.Enums.Scene;
using EggLink.DanhengServer.Game.Scene;
using EggLink.DanhengServer.Game.Scene.Entity;

namespace EggLink.DanhengServer.Game.Battle.Skill.Action;

public class MazeSetTargetMonsterDie : IMazeSkillAction
{
    public async ValueTask OnAttack(AvatarSceneInfo avatar, List<EntityMonster> entities)
    {
        foreach (var entity in entities)
            if (entity.MonsterData.Rank < MonsterRankEnum.Elite)
            {
                await entity.Kill();

                await entity.Scene.Player.LineupManager!.CostMp(1);
                entity.Scene.Player.RogueManager!.GetRogueInstance()?.RollBuff(1);
                entity.Scene.Player.RogueManager!.GetRogueInstance()?.GainMoney(Random.Shared.Next(20, 60));
            }
    }

    public async ValueTask OnCast(AvatarSceneInfo avatar)
    {
        await System.Threading.Tasks.Task.CompletedTask;
    }

    public async ValueTask OnHitTarget(AvatarSceneInfo avatar, List<EntityMonster> entities)
    {
        await System.Threading.Tasks.Task.CompletedTask;
    }
}