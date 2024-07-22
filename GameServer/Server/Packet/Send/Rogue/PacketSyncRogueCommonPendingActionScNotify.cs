using EggLink.DanhengServer.Game.Rogue;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.Server.Packet.Send.Rogue;

public class PacketSyncRogueCommonPendingActionScNotify : BasePacket
{
    public PacketSyncRogueCommonPendingActionScNotify(RogueActionInstance actionInstance, int rogueSubmode) : base(
        CmdIds.SyncRogueCommonPendingActionScNotify)
    {
        var proto = new SyncRogueCommonPendingActionScNotify
        {
            Action = actionInstance.ToProto(),
            RogueSubMode = (uint)rogueSubmode
        };

        SetData(proto);
    }
}