using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.Server.Packet.Send.Mission;

public class PacketFinishItemIdScRsp : BasePacket
{
    public PacketFinishItemIdScRsp(uint itemId) : base(CmdIds.FinishItemIdScRsp)
    {
        var proto = new FinishItemIdScRsp
        {
            ItemId = itemId
        };
        SetData(proto);
    }
}