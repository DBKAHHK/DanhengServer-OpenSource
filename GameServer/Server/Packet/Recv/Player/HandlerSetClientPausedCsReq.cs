using EggLink.DanhengServer.GameServer.Server.Packet.Send.Others;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.Player;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Util;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.Player;

[Opcode(CmdIds.SetClientPausedCsReq)]
public class HandlerSetClientPausedCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        var req = SetClientPausedCsReq.Parser.ParseFrom(data);
        var paused = req.Paused;
        await connection.SendPacket(new PacketSetClientPausedScRsp(paused));
        if (ConfigManager.Config.ServerOption.ServerAnnounce.EnableAnnounce)
            await connection.SendPacket(new PacketServerAnnounceNotify());
    }
}