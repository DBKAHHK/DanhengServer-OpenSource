using EggLink.DanhengServer.Game.Battle;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.Game.Scene.Entity;

public interface IGameEntity
{
    public int EntityID { get; set; }
    public int GroupID { get; set; }

    public ValueTask AddBuff(SceneBuff buff);
    public ValueTask ApplyBuff(BattleInstance instance);


    public SceneEntityInfo ToProto();
}