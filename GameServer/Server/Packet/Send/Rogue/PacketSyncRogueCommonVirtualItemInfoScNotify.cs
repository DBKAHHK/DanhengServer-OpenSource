using EggLink.DanhengServer.GameServer.Game.Rogue;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Rogue;

public class PacketSyncRogueCommonVirtualItemInfoScNotify : BasePacket
{
    public PacketSyncRogueCommonVirtualItemInfoScNotify(BaseRogueInstance instance) : base(
        CmdIds.SyncRogueCommonVirtualItemInfoScNotify)
    {
        var proto = new SyncRogueCommonVirtualItemInfoScNotify
        {
            CommonItemInfo =
            {
                new RogueCommonVirtualItemInfo
                {
                    VirtualItemId = 31,
                    VirtualItemNum = (uint)instance.CurMoney
                }
            }
        };

        SetData(proto);
    }
}