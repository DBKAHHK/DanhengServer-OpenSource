using EggLink.DanhengServer.GameServer.Game.Battle;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Game.Scene.Entity;

public interface IGameEntity
{
    public int EntityId { get; set; }
    public int GroupID { get; set; }

    public List<SceneBuff> BuffList { get; set; }
    public ValueTask AddBuff(SceneBuff buff);
    public ValueTask ApplyBuff(BattleInstance instance);


    public SceneEntityInfo ToProto();
}