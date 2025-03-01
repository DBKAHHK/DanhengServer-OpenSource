using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.EraFlipperData;
public class PacketChangeEraFlipperDataScRsp : BasePacket
{
    public PacketChangeEraFlipperDataScRsp(ChangeEraFlipperDataCsReq req) : base(CmdIds.ChangeEraFlipperDataScRsp)
    {
        var proto = new ChangeEraFlipperDataScRsp
        {
            Data = req.Data
        };

        SetData(proto);
    }
}
