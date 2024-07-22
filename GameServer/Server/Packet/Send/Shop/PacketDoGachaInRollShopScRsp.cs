using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.Server.Packet.Send.Shop;

public class PacketDoGachaInRollShopScRsp : BasePacket
{
    public PacketDoGachaInRollShopScRsp(uint RollShopId, ItemList reward, uint type) : base(
        CmdIds.DoGachaInRollShopScRsp)
    {
        var proto = new DoGachaInRollShopScRsp
        {
            RollShopId = RollShopId,
            MJCIOJJKGMI = type, //Reward type display
            JCPIIANIDML = 0,
            Reward = reward,
            Retcode = 0
        };

        SetData(proto);
    }
}