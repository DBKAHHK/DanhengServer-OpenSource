using EggLink.DanhengServer.GameServer.Game.MatchThree;
using EggLink.DanhengServer.Kcp;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Recv.Lobby;

[Opcode(CmdIds.LobbyQuitCsReq)]
public class HandlerLobbyQuitCsReq : Handler
{
    public override async Task OnHandle(Connection connection, byte[] header, byte[] data)
    {
        await MatchThreeService.QuitRoom(connection.Player!);
        await connection.SendPacket(CmdIds.LobbyQuitScRsp);
    }
}