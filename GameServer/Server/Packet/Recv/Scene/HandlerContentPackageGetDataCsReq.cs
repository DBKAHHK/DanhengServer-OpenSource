using EggLink.DanhengServer.GameServer.Server.Packet.Send.Scene;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.Scene;

[Opcode(CmdIds.ContentPackageGetDataCsReq)]
public class HandlerContentPackageGetDataCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = ContentPackageGetDataCsReq.Parser.ParseFrom(data);

        await connection.SendPacket(
            new PacketContentPackageGetDataScRsp(req
                .ContentId)); // cause crash (not only SR but also ur PC(or other program) 
    }
}