using EggLink.DanhengServer.GameServer.Server.Packet.Send.EraFlipper;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.EraFlipper;

[Opcode(CmdIds.ChangeEraFlipperDataCsReq)]
public class HandlerChangeEraFlipperDataCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = ChangeEraFlipperDataCsReq.Parser.ParseFrom(data);

        var floorId = connection.Player!.SceneInstance!.FloorId;
        await connection.SendPacket(new PacketChangeEraFlipperDataScRsp(req));
        await connection.SendPacket(new PacketEraFlipperDataChangeScNotify(req, floorId));
    }
}