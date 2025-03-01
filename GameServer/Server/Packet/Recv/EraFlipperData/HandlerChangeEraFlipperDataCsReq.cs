using EggLink.DanhengServer.GameServer.Server.Packet.Send.EraFlipperData;
using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Kcp;
using NetTaste;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.EraFlipperData;

[Opcode(CmdIds.ChangeEraFlipperDataCsReq)]
public class HandlerChangeEraFlipperDataCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = ChangeEraFlipperDataCsReq.Parser.ParseFrom(data);

        int floorId = connection.Player!.SceneInstance!.FloorId;
        await connection.SendPacket(new PacketChangeEraFlipperDataScRsp(req));
        await connection.SendPacket(new PacketEraFlipperDataChangeScNotify(req, floorId));
    }
}
