using EggLink.DanhengServer.Database.Inventory;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Shop;

public class PacketSellItemScRsp : BasePacket
{
    public PacketSellItemScRsp(List<ItemData> items) : base(CmdIds.SellItemScRsp)
    {
        var proto = new SellItemScRsp
        {
            ReturnItemList = new ItemList
            {
                ItemList_ = { items.Select(x => x.ToProto()) }
            }
        };

        SetData(proto);
    }
}