using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.Server.Packet.Send.Rogue;

public class PacketSyncRogueFinishScNotify : BasePacket
{
    public PacketSyncRogueFinishScNotify(RogueFinishInfo info) : base(CmdIds.SyncRogueFinishScNotify)
    {
        var proto = new SyncRogueFinishScNotify
        {
            FinishInfo = info
        };

        SetData(proto);
    }
}