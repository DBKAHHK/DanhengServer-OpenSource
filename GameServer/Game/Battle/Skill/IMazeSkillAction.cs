using EggLink.DanhengServer.Game.Scene;
using EggLink.DanhengServer.Game.Scene.Entity;

namespace EggLink.DanhengServer.Game.Battle.Skill;

public interface IMazeSkillAction
{
    public ValueTask OnCast(AvatarSceneInfo avatar);

    public ValueTask OnHitTarget(AvatarSceneInfo avatar, List<EntityMonster> entities);

    public ValueTask OnAttack(AvatarSceneInfo avatar, List<EntityMonster> entities);
}