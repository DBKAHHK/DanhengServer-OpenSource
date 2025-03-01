using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.EraFlipperData;
using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Kcp;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.EraFlipperData;

[Opcode(CmdIds.GetEraFlipperDataCsReq)]
public class HandlerGetEraFlipperDataCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = GetEraFlipperDataCsReq.Parser.ParseFrom(data);

        await connection.SendPacket(new PacketGetEraFlipperDataScRsp(connection.Player!));
    }
}
