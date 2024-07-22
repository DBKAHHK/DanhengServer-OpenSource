using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.Server.Packet.Send.Rogue;

public class PacketSyncRogueExploreWinScNotify : BasePacket
{
    public PacketSyncRogueExploreWinScNotify() : base(CmdIds.SyncRogueExploreWinScNotify)
    {
        var proto = new SyncRogueExploreWinScNotify
        {
            IsWin = true
        };

        SetData(proto);
    }
}