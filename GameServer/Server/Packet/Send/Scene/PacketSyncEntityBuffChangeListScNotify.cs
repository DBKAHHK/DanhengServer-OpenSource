using EggLink.DanhengServer.GameServer.Game.Scene;
using EggLink.DanhengServer.GameServer.Game.Scene.Entity;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Scene;

public class PacketSyncEntityBuffChangeListScNotify : BasePacket
{
    public PacketSyncEntityBuffChangeListScNotify(
        IGameEntity entity, List<SceneBuff> addBuffs, List<SceneBuff> removeBuffs) : base(
        CmdIds.SyncEntityBuffChangeListScNotify)
    {
        var proto = new SyncEntityBuffChangeListScNotify();
        if (addBuffs!= null)
        {
            foreach (var buff in addBuffs)
            {
                var add = new EntityBuffChangeInfo
                {
                    EntityId = (uint)entity.EntityID,
                    BuffChangeInfo = buff.ToProto()
                };
                proto.EntityBuffChangeList.Add(add);
            }
        }
        if (removeBuffs!= null)
        {
            foreach (var buff in removeBuffs)
            {
                var remove = new EntityBuffChangeInfo
                {
                    EntityId = (uint)entity.EntityID,
                    RemoveBuffId = (uint)buff.BuffId
                };
                proto.EntityBuffChangeList.Add(remove);
            }
        }

        SetData(proto);
    }
}