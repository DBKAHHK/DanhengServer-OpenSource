using EggLink.DanhengServer.Database.Inventory;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.Server.Packet.Send.Shop;

public class PacketComposeItemScRsp : BasePacket
{
    public PacketComposeItemScRsp() : base(CmdIds.ComposeItemScRsp)
    {
        var proto = new ComposeItemScRsp
        {
            Retcode = 1
        };

        SetData(proto);
    }

    public PacketComposeItemScRsp(uint composeId, uint count, ItemData item) : base(CmdIds.ComposeItemScRsp)
    {
        var proto = new ComposeItemScRsp
        {
            ReturnItemList = new ItemList
            {
                ItemList_ = { item.ToProto() }
            },
            ComposeId = composeId,
            Count = count
        };

        SetData(proto);
    }
}