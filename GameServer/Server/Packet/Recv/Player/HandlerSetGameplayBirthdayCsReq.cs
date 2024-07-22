using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Server.Packet.Send.Player;

namespace EggLink.DanhengServer.Server.Packet.Recv.Player;

[Opcode(CmdIds.SetGameplayBirthdayCsReq)]
public class HandlerSetGameplayBirthdayCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = SetGameplayBirthdayCsReq.Parser.ParseFrom(data);
        var month = req.Birthday / 100;
        var day = req.Birthday % 100;
        if (month < 1 || month > 12 || day < 1 || day > 31)
        {
            await connection.SendPacket(new PacketSetGameplayBirthdayScRsp());
            return;
        }

        var playerData = connection.Player!.Data;
        if (playerData.Birthday != 0)
        {
            await connection.SendPacket(new PacketSetGameplayBirthdayScRsp());
            return;
        }

        playerData.Birthday = (int)req.Birthday;

        await connection.SendPacket(new PacketSetGameplayBirthdayScRsp(req.Birthday));
    }
}