using EggLink.DanhengServer.GameServer.Server.Packet.Send.FightMatch3;
using EggLink.DanhengServer.Kcp;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.FightMatch3;

[Opcode(CmdIds.FightMatch3DataCsReq)]
public class HandlerFightMatch3DataCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        await connection.SendPacket(new PacketFightMatch3DataScRsp(connection.GameInstance!, connection.Uid));

        connection.GameInstance!.TurnStart();
    }
}