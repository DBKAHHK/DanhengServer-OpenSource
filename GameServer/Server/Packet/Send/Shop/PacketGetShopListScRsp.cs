using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.Shop;

public class PacketGetShopListScRsp : BasePacket
{
    public PacketGetShopListScRsp(uint shopType) : base(CmdIds.GetShopListScRsp)
    {
        var proto = new GetShopListScRsp();

        foreach (var item in GameData.ShopConfigData.Values)
            if (item.ShopType == shopType)
                proto.ShopList.Add(new Proto.Shop
                {
                    ShopId = (uint)item.ShopID,
                    CityLevel = 1,
                    EndTime = long.MaxValue,
                    GoodsList = { item.Goods.Select(g => g.ToProto()) }
                });

        SetData(proto);
    }
}