using EggLink.DanhengServer.GameServer.Server.Packet.Send.Multiplayer;
using EggLink.DanhengServer.Kcp;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.Multiplayer;

[Opcode(CmdIds.MultiplayerGetFightGateCsReq)]
public class HandlerMultiplayerGetFightGateCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        await connection.SendPacket(new PacketMultiplayerGetFightGateScRsp());
    }
}