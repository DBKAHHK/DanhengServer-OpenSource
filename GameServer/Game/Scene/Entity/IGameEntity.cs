using EggLink.DanhengServer.GameServer.Game.Battle;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Game.Scene.Entity;

public interface IGameEntity
{
    public int EntityID { get; set; }
    public int GroupID { get; set; }

    public ValueTask AddBuff(SceneBuff buff);
    public void ApplyBuff(BattleInstance instance);

    public SceneEntityInfo ToProto();
}