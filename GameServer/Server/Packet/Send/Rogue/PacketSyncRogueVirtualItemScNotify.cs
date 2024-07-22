using EggLink.DanhengServer.Game.Rogue;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.Server.Packet.Send.Rogue;

public class PacketSyncRogueVirtualItemScNotify : BasePacket
{
    public PacketSyncRogueVirtualItemScNotify(BaseRogueInstance instance) : base(
        CmdIds.SyncRogueVirtualItemInfoScNotify)
    {
        var proto = new SyncRogueVirtualItemInfoScNotify
        {
            RogueVirtualItemInfo = new RogueVirtualItemInfo
            {
                //RogueMoney = (uint)instance.CurMoney,
            }
        };

        SetData(proto);
    }
}