using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Server.Packet.Send.Challenge;
using Org.BouncyCastle.Ocsp;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;


namespace EggLink.DanhengServer.Server.Packet.Send.Shop
{
    public class PacketDoGachaInRollShopScRsp : BasePacket
    {
        public PacketDoGachaInRollShopScRsp(uint RollShopId, ItemList reward, uint type) : base(CmdIds.DoGachaInRollShopScRsp)
        {
            var proto = new DoGachaInRollShopScRsp();

            proto.RollShopId = RollShopId;
            proto.MJCIOJJKGMI = type; //Reward type display
            proto.JCPIIANIDML = 0;
            proto.Reward = reward;
            proto.Retcode = 0; ;

            SetData(proto);
        }
    }
}
