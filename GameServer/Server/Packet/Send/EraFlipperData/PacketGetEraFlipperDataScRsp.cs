using EggLink.DanhengServer.GameServer.Game.Player;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.EraFlipperData;
public class PacketGetEraFlipperDataScRsp : BasePacket
{
    public PacketGetEraFlipperDataScRsp(PlayerInstance player) : base(CmdIds.GetEraFlipperDataScRsp)
    {
        var proto = new GetEraFlipperDataScRsp();

        SetData(proto);
    }
}
