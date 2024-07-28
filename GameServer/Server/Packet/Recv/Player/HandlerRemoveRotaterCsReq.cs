using EggLink.DanhengServer.Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.Player;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.Player;

[Opcode(CmdIds.RemoveRotaterCsReq)]
public class HandlerRemoveRotaterCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = RemoveRotaterCsReq.Parser.ParseFrom(data);
        await connection.SendPacket(new PacketRemoveRotaterScRsp(connection.Player!, req));
    }
}
